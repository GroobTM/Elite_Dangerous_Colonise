using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;

namespace elite_dangerous_colonise.Classes
{
    public class UpdateHub : Hub
    {
        public static bool isUpdateInProgress { get; private set; } = false;
        public static bool isSearchBlocked { get; private set; } = false;

        private readonly UpdateTimeOptions updateTimeOptions;

        public UpdateHub(IOptions<UpdateTimeOptions> updateTimeOptions)
        {
            this.updateTimeOptions = updateTimeOptions.Value;
        }

        public override async Task OnConnectedAsync()
        {
            string updateStatus = isUpdateInProgress ? "inProgress" : "completed";
            string searchStatus = isSearchBlocked ? "blocked" : "clear";
            await Clients.Caller.SendAsync(
                "SystemUpdateStatus",
                updateStatus,
                searchStatus,
                updateTimeOptions.Hour,
                updateTimeOptions.Minute
            );

            await base.OnConnectedAsync();
        }

        static public void StartUpdate()
        {
            isUpdateInProgress = true;
        }

        static public void BlockSearch()
        {
            isSearchBlocked = true;
        }

        static public void EndUpdate()
        {
            isSearchBlocked = false;
            isUpdateInProgress = false;
        }
    }
}
