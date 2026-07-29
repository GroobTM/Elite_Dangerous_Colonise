using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;

namespace elite_dangerous_colonise.Services
{
    public class UpdateHub : Hub
    {
        private UpdateStatusService updateStatusService;
        private readonly UpdateTimeOptions updateTimeOptions;

        public UpdateHub(UpdateStatusService updateStatusService, IOptions<UpdateTimeOptions> updateTimeOptions)
        {
            this.updateStatusService = updateStatusService;
            this.updateTimeOptions = updateTimeOptions.Value;
        }

        public override async Task OnConnectedAsync()
        {
            (string updateStatus, string searchStatus) = updateStatusService.GetUpdateStatus();

            await Clients.Caller.SendAsync(
                "SystemUpdateStatus",
                updateStatus,
                searchStatus,
                updateTimeOptions.Hour,
                updateTimeOptions.Minute
            );

            await base.OnConnectedAsync();
        }
    }
}
