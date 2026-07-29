using System.Text;
using System.Numerics;
using System.IO.Compression;
using System.Globalization;
using NetMQ.Sockets;
using NetMQ;
using Newtonsoft.Json.Linq;
using Npgsql;
using NpgsqlTypes;

namespace elite_dangerous_colonise.Classes
{
    /// <summary> Defines a EDDNListeningService. </summary>
    public class EDDNListeningService : BackgroundService
    {
        private const string EDDN_ADDRESS = "tcp://eddn.edcd.io:9500";
        private const string JOURNAL_SCHEMA = "https://eddn.edcd.io/schemas/journal/1";

        private readonly NpgsqlDataSource dataSource;
        private readonly AppLogger logger;
        private List<Task> colonyShipUpdates = new List<Task>();
        private List<Task> trailblazerUpdates = new List<Task>();

        /// <summary> Instantiates a EDDNListeningService object. </summary>
        public EDDNListeningService(NpgsqlDataSource dataSource, AppLogger logger)
        {
            this.dataSource = dataSource;
            this.logger = logger;
        }

        private bool IsJournalSchema(string schema)
        {
            return schema == JOURNAL_SCHEMA;
        }

        private bool IsDockedEvent(string messageEvent)
        {
            return messageEvent == "Docked";
        }

        private bool IsSystemColonisationShip(string messageStationName)
        {
            return messageStationName == "System Colonisation Ship" || messageStationName.ToLower().Contains("colonisationship");
        }

        private bool IsTrailblazerMegaship(string messageStationName)
        {
            return messageStationName.Contains("Trailblazer");
        }

        private async Task<string> DecompressMessage(byte[] compressedMessage)
        {
            return await Task.Run(() =>
            {
                using (MemoryStream memoryStream = new MemoryStream(compressedMessage))
                using (ZLibStream decompressor = new ZLibStream(memoryStream, CompressionMode.Decompress))
                using (StreamReader streamReader = new StreamReader(decompressor, Encoding.UTF8))
                {
                    return streamReader.ReadToEnd();
                }
            });
        }

        private Vector3 ConvertJsonToVector(JToken token)
        {
            float[] coords = token["StarPos"].ToObject<float[]>();
            return new Vector3(coords[0], coords[1], coords[2]);
        }

        private async Task UpdateColonisingTracker(JToken message)
        {
            try
            {
                Vector3 coords = ConvertJsonToVector(message);

                if (SolDistanceChecker.InRangeOfSol(coords))
                {
                    if (long.TryParse(message["SystemAddress"].ToString(), out long systemID)
                        && DateTime.TryParse(message["timestamp"].ToString(), null, DateTimeStyles.AdjustToUniversal, out DateTime timestamp))
                    {
                        timestamp = DateTime.SpecifyKind(timestamp, DateTimeKind.Utc);
                        await UpdateColonisationDatabase(systemID, timestamp);

                        logger.LogInformation("EDDN Listening Service", 1, $"Updating system {systemID}.");
                    }
                        
                }
            }
            catch (Exception ex)
            {
                logger.LogError("EDDN Listening Service", 2, "Error occured while trying to update a system.", ex);
            }
        }

        private async Task UpdateColonisationDatabase(long systemID, DateTime timestamp)
        {
            await using (NpgsqlConnection conn = await dataSource.OpenConnectionAsync())
            {
                await using NpgsqlCommand command = new NpgsqlCommand("SELECT \"ClaimStarSystem\"(@inputSystemID, @inputClaimDate)", conn);
                
                command.Parameters.AddWithValue("inputSystemID", NpgsqlDbType.Bigint, systemID);
                command.Parameters.AddWithValue("inputClaimDate", NpgsqlDbType.TimestampTz, timestamp);

                await command.ExecuteNonQueryAsync();
            }
        }

        private async Task UpdateTrailblazer(JToken message)
        {
            ulong[] validStations =
            {
                129033207,
                129032951,
                129032695,
                129032183,
                129033463,
                129032439,
            };

            try
            {
                if (ulong.TryParse(message?["MarketID"].ToString(), out ulong stationID) && validStations.Contains(stationID))
                {
                    Vector3 coords = ConvertJsonToVector(message);
                    string name = message["StationName"].ToString();

                    if (DateTime.TryParse(message["timestamp"].ToString(), null, DateTimeStyles.AdjustToUniversal, out DateTime timestamp))
                    {
                        timestamp = DateTime.SpecifyKind(timestamp, DateTimeKind.Utc);
                        await UpdateTrailblazerDatabase(stationID, name, coords, timestamp);

                        logger.LogInformation("EDDN Listening Service", 8, $"Updating {name}.");
                    }                    
                }
            }
            catch (Exception ex)
            {
                logger.LogError("EDDN Listening Service", 9, "Error occured while trying to update a Trailblazer megaship.", ex);
            }
        }

        private async Task UpdateTrailblazerDatabase(ulong trailblazerID, string trailblazerName, Vector3 coordinates, DateTime timestamp)
        {
            await using (NpgsqlConnection conn = await dataSource.OpenConnectionAsync())
            {
                await using NpgsqlCommand command = new NpgsqlCommand(
                    "SELECT \"InsertTrailblazerMegaship\"(@inputID, @inputName, @inputCoordinateX, @inputCoordinateY, @inputCoordinateZ, @inputUpdateDate)", conn);

                command.Parameters.AddWithValue("inputID", NpgsqlDbType.Numeric, trailblazerID);
                command.Parameters.AddWithValue("inputName", NpgsqlDbType.Varchar, trailblazerName);
                command.Parameters.AddWithValue("inputCoordinateX", NpgsqlDbType.Numeric, coordinates.X);
                command.Parameters.AddWithValue("inputCoordinateY", NpgsqlDbType.Numeric, coordinates.Y);
                command.Parameters.AddWithValue("inputCoordinateZ", NpgsqlDbType.Numeric, coordinates.Z);
                command.Parameters.AddWithValue("inputUpdateDate", NpgsqlDbType.TimestampTz, timestamp);

                command.CommandTimeout = 120;
                await command.ExecuteNonQueryAsync();
            }
        }

        private async Task RemoveClaim(JToken message)
        {
            try
            {
                Vector3 coords = ConvertJsonToVector(message);

                if (SolDistanceChecker.InRangeOfSol(coords))
                {
                    if (long.TryParse(message["SystemAddress"].ToString(), out long systemID)
                        && DateTime.TryParse(message["timestamp"].ToString(), null, DateTimeStyles.AdjustToUniversal, out DateTime timestamp))
                    {
                        await RemoveClaimFromDatabase(systemID, timestamp);

                        logger.LogInformation("EDDN Listening Service", 10, $"Removed claim from system {systemID}.");
                    }

                }
            }
            catch (Exception ex)
            {
                logger.LogError("EDDN Listening Service", 11, "Error occured while trying to remove a claim from a system.", ex);
            }
        }

        private async Task RemoveClaimFromDatabase(long systemID, DateTime timestamp)
        {
            await using (NpgsqlConnection conn = await dataSource.OpenConnectionAsync())
            {
                await using NpgsqlCommand command = new NpgsqlCommand("SELECT \"UnclaimStarSystem\"(@inputSystemID, @inputUnclaimDate)", conn);

                command.Parameters.AddWithValue("inputSystemID", NpgsqlDbType.Bigint, systemID);
                command.Parameters.AddWithValue("inputUnclaimDate", NpgsqlDbType.TimestampTz, timestamp);

                await command.ExecuteNonQueryAsync();
            }
        }

        private async Task ListenToEDDN(CancellationToken cancellationToken)
        {
            using (SubscriberSocket subscriber = new SubscriberSocket())
            {
                subscriber.Connect(EDDN_ADDRESS);
                subscriber.Subscribe(string.Empty);

                logger.LogInformation("EDDN Listening Service", 0, $"Connected to {EDDN_ADDRESS}, listening for messages.");

                while (!cancellationToken.IsCancellationRequested)
                {
                    colonyShipUpdates.RemoveAll(task => task.IsCompleted || task.IsFaulted || task.IsCanceled);
                    trailblazerUpdates.RemoveAll(task => task.IsCompleted || task.IsFaulted || task.IsCanceled);

                    try
                    {
                        JObject jsonContent = JObject.Parse(await DecompressMessage(subscriber.ReceiveFrameBytes()));

                        JToken message = jsonContent["message"];
                        string schema = jsonContent?["$schemaRef"]?.ToString();
                        string messageEvent = message?["event"]?.ToString();
                        string messageStationName = message?["StationName"]?.ToString();                        

                        if (IsJournalSchema(schema) && IsDockedEvent(messageEvent))
                        {
                            if (IsSystemColonisationShip(messageStationName))
                            {
                                Task task = Task.Run(async () => await UpdateColonisingTracker(message), cancellationToken);

                                colonyShipUpdates.Add(task);
                            }
                            //else if (IsTrailblazerMegaship(messageStationName))
                            //{
                            //    Task task = Task.Run(async () => await UpdateTrailblazer(message), cancellationToken);

                            //    trailblazerUpdates.Add(task);
                            //}
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.LogError("EDDN Listening Service", 3, "Error occured while receiving or processing message.", ex);
                    }
                }
            }
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            await Task.Run(() => ListenToEDDN(cancellationToken), cancellationToken);
        }

        /// <summary>
        /// Waits for the background tasks to complete and then stops the background service.
        /// </summary>
        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("EDDN Listening Service", 4, "Waiting for background tasks to complete.");

            try
            {
                using (CancellationTokenSource timeoutToken = new CancellationTokenSource(TimeSpan.FromMinutes(2)))
                {
                    await Task.WhenAll(colonyShipUpdates).WaitAsync(timeoutToken.Token);
                    logger.LogInformation("EDDN Listening Service", 5, "All background tasks completed. Service shutting down.");
                }
            }
            catch (OperationCanceledException)
            {
                logger.LogWarning("EDDN Listening Service", 6, "Background tasks took too long to complete. Forcing shutdown.");
            }
            catch (Exception ex)
            {
                logger.LogError("EDDN Listening Service", 7, "An error occured while waiting for background tasks to complete.", ex);
            }
        }
    }
}
