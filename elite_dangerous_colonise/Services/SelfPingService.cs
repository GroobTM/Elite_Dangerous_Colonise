namespace elite_dangerous_colonise.Services
{
    /// <summary>
    /// Defines the SelfPingService background service.
    /// </summary>
    public class SelfPingService : BackgroundService
    {
        private readonly IHttpClientFactory httpClientFactory;
        private readonly AppLogger logger;

        /// <summary>
        /// Constructs a SelfPingService object.
        /// </summary>
        public SelfPingService(IHttpClientFactory httpClientFactory, AppLogger logger)
        {
            this.httpClientFactory = httpClientFactory;
            this.logger = logger;
        }

        /// <summary>
        /// Pings https://edcolonise.net/ every 10 minutes to keep it awake.
        /// </summary>
        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Self Ping Service", 0, "Self Ping Service starting.");


            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    using (HttpClient client = httpClientFactory.CreateClient())
                    {
                        HttpResponseMessage response = await client.GetAsync("https://edcolonise.net/", cancellationToken);
                        if (response.IsSuccessStatusCode)
                        {
                            logger.LogInformation("Self Ping Service", 1, "Self ping was successful.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError("Self Ping Service", 2, "Self ping failed and caused an error.", ex);
                }

                await Task.Delay(TimeSpan.FromMinutes(10), cancellationToken);
            }

            logger.LogInformation("Self Ping Service", 3, "Self Ping Service stopping.");
        }
    }
}
