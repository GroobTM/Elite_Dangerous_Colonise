using Npgsql;
using Newtonsoft.Json;
using elite_dangerous_colonise.Models.Json_Structure;

namespace elite_dangerous_colonise.Classes
{
    /// <summary>
    /// Defines a DatabaseBulkWriter.
    /// </summary>
    public class DatabaseBulkWriter
    {
        private const int BULK_SIZE = 2000;

        private readonly bool verboseReporting;
        private readonly NpgsqlDataSource dataSource;
        private readonly AppLogger logger;
        private int recordsRead = 0;
        private int recordsAddedOrUpdated = 0;
        private int recordsFailedToAdd = 0;


        /// <summary>
        /// Instantiates a DatabaseBulkWriter.
        /// </summary>
        /// <param name="dataSource"> The database datasource. </param>
        public DatabaseBulkWriter(NpgsqlDataSource dataSource, AppLogger logger)
        {
            this.dataSource = dataSource;
            this.logger = logger;
            verboseReporting = true;
        }
        /// <inheritdoc cref="DatabaseBulkWriter.DatabaseBulkWriter(NpgsqlDataSource)"/>
        /// <param name="verboseReporting"> If the database writer should report its reading progress. </param>
        public DatabaseBulkWriter(NpgsqlDataSource dataSource, AppLogger logger, bool verboseReporting)
        {
            this.dataSource = dataSource;
            this.logger = logger;
            this.verboseReporting = verboseReporting;
        }

        private async Task InsertStarSystemsBulk(NpgsqlConnection conn, NpgsqlTransaction transaction,
            DatabaseDataLists dataLists)
        {
            await using (NpgsqlCommand command = new NpgsqlCommand("SELECT \"InsertStarSystemsBulk\"(@inputStarSystems)", conn, transaction))
            {
                command.Parameters.AddWithValue("inputStarSystems", dataLists.StarSystems.ToArray());

                await command.ExecuteNonQueryAsync();
            }
        }

        private async Task InsertStationsBulk(NpgsqlConnection conn, NpgsqlTransaction transaction,
            DatabaseDataLists dataLists)
        {
            await using(NpgsqlCommand command = new NpgsqlCommand("SELECT \"InsertStationsBulk\"(@inputStations)", conn, transaction))
            {
                command.Parameters.AddWithValue("inputStations", dataLists.Stations.ToArray());

                await command.ExecuteNonQueryAsync();
            }
        }

        private async Task InsertUncolonisedStarSystemDetailsBulk(NpgsqlConnection conn, NpgsqlTransaction transaction,
            DatabaseDataLists dataLists)
        {
            await using(NpgsqlCommand command = new NpgsqlCommand("SELECT \"InsertUncolonisedStarSystemDetailsBulk\"(@inputDetails)", conn, transaction))
            {
                command.Parameters.AddWithValue("inputDetails", dataLists.UncolonisedDetails.ToArray());

                await command.ExecuteNonQueryAsync();
            }
        }

        private async Task InsertRingsBulk(NpgsqlConnection conn, NpgsqlTransaction transaction,
            DatabaseDataLists dataLists)
        {
            await using(NpgsqlCommand command = new NpgsqlCommand("SELECT \"InsertRingsBulk\"(@inputRings)", conn, transaction))
            {
                command.Parameters.AddWithValue("inputRings", dataLists.Rings.ToArray());

                await command.ExecuteNonQueryAsync();
            }
        }

        private async Task InsertHotspotsBulk(NpgsqlConnection conn, NpgsqlTransaction transaction,
            DatabaseDataLists dataLists)
        {
            await using (NpgsqlCommand command = new NpgsqlCommand("SELECT \"InsertHotspotsBulk\"(@inputHotspots)", conn, transaction))
            {
                command.Parameters.AddWithValue("inputHotspots", dataLists.Hotspots.ToArray());

                await command.ExecuteNonQueryAsync();
            }
        }

        private void ReportInsert(List<string> insertedNames)
        {
            if (verboseReporting)
            {
                foreach (string name in insertedNames)
                {
                    logger.LogInformation("Database Writer", 4, $"System {name} was added to the database.");
                }
            }
        }

        private async Task BulkInsertIntoDatabase(DatabaseDataLists dataLists)
        {
            await using (NpgsqlConnection conn = await dataSource.OpenConnectionAsync())
            {
                await using (NpgsqlTransaction transaction = await conn.BeginTransactionAsync())
                {
                    try
                    {
                        dataLists.Deduplicate();
                        await InsertStarSystemsBulk(conn, transaction, dataLists);
                        await InsertStationsBulk(conn, transaction, dataLists);
                        await InsertUncolonisedStarSystemDetailsBulk(conn, transaction, dataLists);
                        await InsertRingsBulk(conn, transaction, dataLists);
                        await InsertHotspotsBulk(conn, transaction, dataLists);

                        await transaction.CommitAsync();
                        recordsAddedOrUpdated += dataLists.Count();

                        if (verboseReporting) 
                        {
                            ReportInsert(dataLists.GetNames());
                        }
                    }
                    catch (NpgsqlException ex)
                    {
                        recordsFailedToAdd += dataLists.Count();
                        logger.LogError("Database Writer", 5, ex);

                        try
                        {
                            await transaction.RollbackAsync();
                        }
                        catch (InvalidOperationException rollbackEx)
                        {
                            logger.LogError("Database Writer", 6, rollbackEx);
                        }
                        
                    }
                }
            }
        }

        private async Task ReportReading(CancellationToken token)
        {
            DateTime startTime = DateTime.Now;

            try
            {
                while (!token.IsCancellationRequested)
                {
                    logger.LogInformation("Database Writer", 100, $"Reading In Progress " +
                        $"| Records Read: {recordsRead} " +
                        $"({recordsRead / Math.Max(1, DateTime.Now.Subtract(startTime).TotalMinutes)}/min) " +
                        $"| Records Added: {recordsAddedOrUpdated} " +
                        $"({recordsAddedOrUpdated / Math.Max(1, DateTime.Now.Subtract(startTime).TotalMinutes)}/min) " +
                        $"| Records Failed to Add: {recordsFailedToAdd} " +
                        $"({recordsFailedToAdd / Math.Max(1, DateTime.Now.Subtract(startTime).TotalMinutes)}/min)");
                    await Task.Delay(TimeSpan.FromMinutes(1), token);
                }
            }
            catch (TaskCanceledException)
            {
                logger.LogInformation("Database Writer", 101, $"Reading Complete " +
                    $"| Records Read: {recordsRead} " +
                    $"({recordsRead / Math.Max(1, DateTime.Now.Subtract(startTime).TotalMinutes)}/min) " +
                    $"| Records Added: {recordsAddedOrUpdated} " +
                    $"({recordsAddedOrUpdated / Math.Max(1, DateTime.Now.Subtract(startTime).TotalMinutes)}/min) " +
                    $"| Records Failed to Add: {recordsFailedToAdd} " +
                    $"({recordsFailedToAdd / Math.Max(1, DateTime.Now.Subtract(startTime).TotalMinutes)}/min)");
            }
        }

        /// <summary> Reads a Json file and inserts its data into the database. </summary>
        /// <param name="filePath"> The file path of a Json file. </param>
        /// <remarks> This method reports it's progress periodically. </remarks>
        public async Task InsertJsonIntoDatabase(string filePath)
        {
            try
            {
                using StreamReader streamReader = new StreamReader(filePath);
                await InsertJsonIntoDatabase(streamReader);

            }
            catch (JsonReaderException ex)
            {
                logger.LogError("Database Writer", 0, ex);
            }
            catch (UnauthorizedAccessException ex)
            {
                logger.LogError("Database Writer", 1, ex);
            }
            catch (Exception ex)
            {
                logger.LogError("Database Writer", 2, ex);
            }
        }

        /// <summary> Reads a streamed Json file and inserts its data into the database. </summary>
        /// <param name="inputStream"> The stream of a Json file. </param>
        /// <remarks> This method reports it's progress periodically. </remarks>
        public async Task InsertJsonIntoDatabase(Stream inputStream)
        {
            try
            {
                using StreamReader streamReader = new StreamReader(inputStream, bufferSize: 81920);
                await InsertJsonIntoDatabase(streamReader);

            }
            catch (JsonReaderException ex)
            {
                logger.LogError("Database Writer", 0, ex);
            }
            catch (IOException ex)
            {
                logger.LogError("Database Writer", 1, ex);
            }
            catch (Exception ex)
            {
                logger.LogError("Database Writer", 2, ex);
            }
        }

        private async Task InsertJsonIntoDatabase(StreamReader streamReader)
        {
            using (JsonTextReader jsonReader = new JsonTextReader(streamReader))
            {
                jsonReader.CloseInput = false;
                JsonSerializer serializer = new JsonSerializer();

                CancellationTokenSource cancellationToken = new CancellationTokenSource();
                Task readingReportingTask = ReportReading(cancellationToken.Token);

                DatabaseDataLists dataLists = new DatabaseDataLists();

                while (await jsonReader.ReadAsync())
                {
                    if (jsonReader.TokenType == JsonToken.StartObject)
                    {
                        SystemJson? systemJson = serializer.Deserialize<SystemJson>(jsonReader);

                        if (systemJson != null && SolDistanceChecker.InRangeOfSol(systemJson.Coordinates))
                        {
                            StarSystem? decodedSystem = systemJson.ConvertToStarSystem();
                            if (decodedSystem != null)
                            {
                                decodedSystem.AddToDataLists(dataLists);
                            }

                            if (dataLists.Count() >= BULK_SIZE)
                            {
                                await BulkInsertIntoDatabase(dataLists);
                                dataLists.ClearLists();
                            }
                        }
                        recordsRead++;
                    }
                }

                if (dataLists.Count() > 0)
                {
                    await BulkInsertIntoDatabase(dataLists);
                }

                cancellationToken.Cancel();
                await readingReportingTask;
            }
        }
    }
}
