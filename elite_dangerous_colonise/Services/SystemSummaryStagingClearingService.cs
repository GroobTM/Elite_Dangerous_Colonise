using Microsoft.AspNetCore.SignalR;
using Npgsql;

namespace elite_dangerous_colonise.Services
{
    /// <summary> Converts the staged changes to Star Systems in the database into live records and rebuilds saved queries. </summary>
    public class SystemSummaryStagingClearingService : BackgroundService
    {
        private readonly NpgsqlDataSource dataSource;
        private readonly SpanshDataDumpDownloadService spanshService;
        private readonly IHubContext<UpdateHub> hubContext;
        private readonly UpdateStatusService updateStatusService;
        private readonly AppLogger logger;

        /// <summary> Instantiates a SystemSummaryStagingClearingService. </summary>
        /// <param name="dataSource"> The database data source. </param>
        /// <param name="spanshService"> The Spansh datadump download service. </param>
        /// <param name="hubContext"> The UpdateHub's context. </param>
        /// <param name="updateStatusService"> The update status service. </param>
        /// <param name="logger"> The logger service. </param>
        public SystemSummaryStagingClearingService(NpgsqlDataSource dataSource, SpanshDataDumpDownloadService spanshService,
            IHubContext<UpdateHub> hubContext, UpdateStatusService updateStatusService, AppLogger logger)
        {
            this.dataSource = dataSource;
            this.spanshService = spanshService;
            this.hubContext = hubContext;
            this.updateStatusService = updateStatusService;
            this.logger = logger;
        }

        private async Task InsertColonisableStarSystemsFromStaged(NpgsqlConnection conn, bool insertColonised)
        {
            await using (NpgsqlCommand command = new NpgsqlCommand("SELECT \"InsertColonisableStarSystemsFromStaged\"(@insertColonised)", conn))
            {
                command.CommandTimeout = 180;
                command.Parameters.AddWithValue("insertColonised", insertColonised);
                await command.ExecuteNonQueryAsync();
            }
        }

        private async Task TruncateStagedStarSystems(NpgsqlConnection conn)
        {
            await using (NpgsqlCommand command = new NpgsqlCommand("TRUNCATE \"StagedStarSystems\"", conn))
            {
                await command.ExecuteNonQueryAsync();
            }
        }

        private async Task ProcessStagedSystemSummaries(NpgsqlConnection conn)
        {
            using (NpgsqlTransaction transaction = await conn.BeginTransactionAsync())
            {
                try
                {
                    await InsertColonisableStarSystemsFromStaged(conn, true);
                    logger.LogInformation("System Summary Staging Clearing Service", 2, "Colonised staged systems added.");
                    await InsertColonisableStarSystemsFromStaged(conn, false);
                    logger.LogInformation("System Summary Staging Clearing Service", 3, "Uncolonised staged systems added.");
                    await TruncateStagedStarSystems(conn);

                    await transaction.CommitAsync();

                    logger.LogInformation("System Summary Staging Clearing Service", 4, "Processing Complete.");
                }
                catch (NpgsqlException ex)
                {
                    await transaction.RollbackAsync();
                    logger.LogError("System Summary Staging Clearing Service", 6, ex);
                }
            }
        }

        private async Task RefreshDistinctColonisedStarSystems(NpgsqlConnection conn)
        {
            logger.LogInformation("System Summary Staging Clearing Service", 9, "Refreshing DistinctColonisedStarSystems view.");

            await using (NpgsqlCommand command = new NpgsqlCommand("SELECT \"RefreshDistinctColonisedStarSystems\"()", conn))
            {
                await command.ExecuteNonQueryAsync();
            }
        }

        private async Task RefreshDistinctUncolonisedStarSystems(NpgsqlConnection conn)
        {
            logger.LogInformation("System Summary Staging Clearing Service", 9, "Refreshing DistinctUncolonisedStarSystems view.");

            await using (NpgsqlCommand command = new NpgsqlCommand("SELECT \"RefreshDistinctUncolonisedStarSystems\"()", conn))
            {
                await command.ExecuteNonQueryAsync();
            }
        }

        private async Task RefreshMaxSearchValues(NpgsqlConnection conn)
        {
            logger.LogInformation("System Summary Staging Clearing Service", 9, "Refreshing MaxSearchValues view.");

            await using (NpgsqlCommand command = new NpgsqlCommand("SELECT \"RefreshMaxSearchValues\"()", conn))
            {
                await command.ExecuteNonQueryAsync();
            }
        }

        private async Task RefreshValuesTables(NpgsqlConnection conn)
        {
            await using (NpgsqlTransaction transaction = await conn.BeginTransactionAsync())
            {
                await RefreshDistinctColonisedStarSystems(conn);
                await RefreshDistinctUncolonisedStarSystems(conn);
                await RefreshMaxSearchValues(conn);

                await transaction.CommitAsync();

                logger.LogInformation("System Summary Staging Clearing Service", 11, "Update complete.");
            }
        }

        private async Task OnDataDumpProcessingComplete(object? sender, EventArgs e)
        {
            logger.LogInformation("System Summary Staging Clearing Service", 1, "Running staged system processing.");
            try
            {
                await using (NpgsqlConnection conn = await dataSource.OpenConnectionAsync())
                {
                    await ProcessStagedSystemSummaries(conn);

                    updateStatusService.BlockSearch();
                    await hubContext.Clients.All.SendAsync("SearchBlockEnabled");

                    await RefreshValuesTables(conn);
                }
            }
            catch (Exception ex)
            {
                logger.LogError("System Summary Staging Clearing Service", 7, ex);
            }
            finally
            {
                updateStatusService.EndUpdate();
                await hubContext.Clients.All.SendAsync("SystemUpdateComplete");
            }
        }

        /// <summary> Awaits the DataDumpProcessingComplete event and then stages new database changes and rebuilds saved queries. </summary>
        protected override Task ExecuteAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("System Summary Staging Clearing Service", 0, "Staging Clearing Service starting.");

            spanshService.DataDumpProcessingComplete += (sender, e) => Task.Run(async () => await OnDataDumpProcessingComplete(sender, e));

            return Task.CompletedTask;
        }

        /// <summary> Stops the background service. </summary>
        public override Task StopAsync(CancellationToken cancellationToken)
        { 
            spanshService.DataDumpProcessingComplete -= (sender, e) => Task.Run(async () => await OnDataDumpProcessingComplete(sender, e));

            logger.LogInformation("System Summary Staging Clearing Service", 8, "Staging Clearing Service stopped.");

            return Task.CompletedTask;
        }
    }
}
