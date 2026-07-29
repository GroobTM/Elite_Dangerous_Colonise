using System.IO.Compression;
using elite_dangerous_colonise.Models.Internal;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;


namespace elite_dangerous_colonise.Services
{
    /// <summary> A service that streams the daily Spansh data dump. </summary>
    public class SpanshDataDumpDownloadService : BackgroundService
    {
        private const string DOWNLOAD_URL = "https://downloads.spansh.co.uk/galaxy_1day.json.gz";
        
        private static readonly HttpClient client = new HttpClient();
        private readonly IServiceScopeFactory scopeFactory;
        private readonly IHubContext<UpdateHub> hubContext;
        private readonly UpdateStatusService updateStatusService;
        private readonly AppLogger logger;
        private readonly UpdateTimeOptions updateTimeOptions;

        /// <summary> Event that signals the daily data dump has been fully streamed and processed. </summary>
        public event EventHandler? DataDumpProcessingComplete;


        /// <summary> Instantiates a SpanshDataDumpDownloadService. </summary>
        /// <param name="scopeFactory"> The configured scope factory service. </param>
        /// <param name="hubContext"> The UpdateHub's context. </param>
        /// <param name="updateStatusService"> The update status service. </param>
        /// <param name="logger"> The logger service. </param>
        /// <param name="updateTimeOptions"> The update time options.</param>
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

        /// <summary> Waits for the next update date and then begins streaming the daily Spansh data dump. Repeats daily. </summary>
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
        public override Task StopAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Spansh Download Service", 11, "Spansh data dump download service stopped.");

            return Task.CompletedTask;
        }
    }
}
