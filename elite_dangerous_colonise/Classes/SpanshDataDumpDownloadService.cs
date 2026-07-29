using System.IO.Compression;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;


namespace elite_dangerous_colonise.Classes
{
    /// <summary> Defines a SpanshDataDumpDownloadService. </summary>
    public class SpanshDataDumpDownloadService : BackgroundService
    {
        private const string DOWNLOAD_URL = "https://downloads.spansh.co.uk/galaxy_1day.json.gz";
        
        private static readonly HttpClient client = new HttpClient();
        private readonly IServiceScopeFactory scopeFactory;
        private readonly IHubContext<UpdateHub> hubContext;
        private readonly UpdateStatusService updateStatusService;
        private readonly AppLogger logger;
        private readonly UpdateTimeOptions updateTimeOptions;

        public event EventHandler? DataDumpProcessingComplete;

        /// <summary> Instantiates a SpanshDataDumpDownloadService object. </summary>
        public SpanshDataDumpDownloadService(IServiceScopeFactory scopeFactory, IHubContext<UpdateHub> hubContext, UpdateStatusService updateStatusService, AppLogger logger, IOptions<UpdateTimeOptions> updateTimeOptions)
        {
            this.scopeFactory = scopeFactory;
            this.hubContext = hubContext;
            this.updateStatusService = updateStatusService;
            this.logger = logger;
            this.updateTimeOptions = updateTimeOptions.Value;
        }

        private TimeSpan TimeUntilStart()
        {
            DateTime currentTime = DateTime.UtcNow;

            DateTime startTime = new DateTime(currentTime.Year, currentTime.Month, currentTime.Day,
                updateTimeOptions.Hour, updateTimeOptions.Minute, 0, DateTimeKind.Utc);

            if (currentTime > startTime)
            {
                startTime = startTime.AddDays(1);
            }

            return startTime - currentTime;
        }

        private async Task DownloadAndProcessDataDump()
        {
            int maxAttempts = 3;
            int attemptDelay = 5;

            try
            {
                updateStatusService.StartUpdate();
                await hubContext.Clients.All.SendAsync("SystemUpdateStarted");

                for (int attempt = 1; attempt <= maxAttempts; attempt++)
                {
                    try
                    {
                        logger.LogInformation("Spansh Download Service", 1, $"Spansh data dump download and processing attempt {attempt} starting.");
                        await using (AsyncServiceScope scope = scopeFactory.CreateAsyncScope())
                        {
                            DatabaseBulkWriter dbWriter = scope.ServiceProvider.GetRequiredService<DatabaseBulkWriter>();

                            using (HttpResponseMessage response = await client.GetAsync(DOWNLOAD_URL, HttpCompletionOption.ResponseHeadersRead))
                            {
                                response.EnsureSuccessStatusCode();
                                await using (Stream networkStream = await response.Content.ReadAsStreamAsync())
                                await using (GZipStream decompressionStream = new GZipStream(networkStream, CompressionMode.Decompress))
                                {
                                    await dbWriter.InsertJsonIntoDatabase(decompressionStream);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        if (attempt < maxAttempts)
                        {
                            logger.LogError("Spansh Download Service", 2, $"Spansh data dump download attempt {attempt} failed.", ex);

                            await Task.Delay(TimeSpan.FromSeconds(attemptDelay));
                        }
                        else
                        {
                            throw;
                        }
                    }

                    logger.LogInformation("Spansh Download Service", 3, "Spansh data dump download and processing complete.");
                    DataDumpProcessingComplete?.Invoke(this, EventArgs.Empty);
                    return;
                }
            }
            catch (Exception ex)
            {
                logger.LogError("Spansh Download Service", 10, "Spansh data dump download and processing failed.", ex);
            }
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Spansh Download Service", 0, "Spansh data dump download service started.");

            var startDelay = TimeUntilStart();

            await Task.Delay(startDelay, cancellationToken);

            while (!cancellationToken.IsCancellationRequested)
            {
                await DownloadAndProcessDataDump();

                await Task.Delay(TimeSpan.FromDays(1), cancellationToken);
            }
        }

        /// <summary> Stops the background service. </summary>
        /// <returns> A completed task. </returns>
        public override Task StopAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Spansh Download Service", 11, "Spansh data dump download service stopped.");

            return Task.CompletedTask;
        }
    }
}
