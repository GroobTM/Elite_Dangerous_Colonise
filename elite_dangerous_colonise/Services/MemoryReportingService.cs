using System.Diagnostics;

namespace elite_dangerous_colonise.Services
{
    public class MemoryReportingService : BackgroundService
    {
        private readonly AppLogger logger;

        public MemoryReportingService(AppLogger logger)
        {
            this.logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            ReportUsage("Service Started");
            
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
                    ReportUsage("Hourly Report");
                }
                catch (TaskCanceledException) { }
            }
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            ReportUsage("Service Stopped");
            return base.StopAsync(cancellationToken);
        }

        private void ReportUsage(string context)
        {
            try
            {
                Process proc = Process.GetCurrentProcess();
                proc.Refresh();

                long physicalMemory = proc.WorkingSet64 / 1024 / 1024;
                long privateMemory = proc.PrivateMemorySize64 / 1024 / 1024;
                long heap = GC.GetTotalMemory(false) / 1024 / 1024;

                logger.LogInformation("Memory Reporting Service", 1,$"{context} | Physical Memory: {physicalMemory}MB | Private Memory: {privateMemory}MB | Managed Heap: {heap}MB");
            }
            catch (Exception ex)
            {
                logger.LogError("Memory Reporting Service", 2, ex);
            }
        }
    }
}
