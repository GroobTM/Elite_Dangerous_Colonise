using elite_dangerous_colonise.Models.Internal;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;

namespace elite_dangerous_colonise.Services
{
    /// <summary> SignalR hub that reports the current status of the database update. </summary>
    public class UpdateHub : Hub
    {
        private UpdateStatusService updateStatusService;
        private readonly UpdateTimeOptions updateTimeOptions;

        /// <summary> Instantiates a UpdateHub. </summary>
        /// <param name="updateStatusService"> The update status service. </param>
        /// <param name="updateTimeOptions"> The update time options.</param>
        public UpdateHub(UpdateStatusService updateStatusService, IOptions<UpdateTimeOptions> updateTimeOptions)
        {
            this.updateStatusService = updateStatusService;
            this.updateTimeOptions = updateTimeOptions.Value;
        }

        /// <summary> Reports the current status of the database update and the time until the next update. </summary>
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
