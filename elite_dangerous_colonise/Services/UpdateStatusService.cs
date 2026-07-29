namespace elite_dangerous_colonise.Services
{
    public class UpdateStatusService
    {
        public bool IsUpdateInProgress { get; private set; } = false;
        public bool IsSearchBlocked { get; private set; } = false;

        private readonly object objLock = new();

        public (string updateStatus, string searchStatus) GetUpdateStatus()
        {
            lock (objLock)
            {
                string updateStatus = IsUpdateInProgress ? "inProgress" : "completed";
                string searchStatus = IsSearchBlocked ? "blocked" : "clear";
                return (updateStatus, searchStatus);
            }
        }

        public void StartUpdate()
        {
            lock (objLock)
            {
                IsUpdateInProgress = true;
            }
        }

        public void BlockSearch()
        {
            lock (objLock)
            {
                IsSearchBlocked = true;
            }
        }

        public void EndUpdate()
        {
            lock (objLock)
            {
                IsSearchBlocked = false;
                IsUpdateInProgress = false;
            }
        }
    }
}
