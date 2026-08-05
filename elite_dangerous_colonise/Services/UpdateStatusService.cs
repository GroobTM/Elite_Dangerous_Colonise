namespace elite_dangerous_colonise.Services
{
    /// <summary> Tracks the status of the database update. </summary>
    public class UpdateStatusService
    {
        /// <summary> If the database update is currently in progress. </summary>
        public bool IsUpdateInProgress { get; private set; } = false;
        /// <summary> If the database is currently searchable. </summary>
        public bool IsSearchBlocked { get; private set; } = false;

        private readonly object objLock = new();

        /// <summary> Returns the current status of the database update. </summary>
        /// <returns>
        /// <b>updateStaus:</b> If the database update is "inProgress" or "completed".
        /// <br></br>
        /// <b>searchStatus:</b> If the database search is "blocked" or "clear".
        /// </returns>
        public (string updateStatus, string searchStatus) GetUpdateStatus()
        {
            lock (objLock)
            {
                string updateStatus = IsUpdateInProgress ? "inProgress" : "completed";
                string searchStatus = IsSearchBlocked ? "blocked" : "clear";
                return (updateStatus, searchStatus);
            }
        }

        /// <summary> Sets the database update status to be in progress. </summary>
        public void StartUpdate()
        {
            lock (objLock)
            {
                IsUpdateInProgress = true;
            }
        }

        /// <summary> Sets the search block status to be blocked. </summary>
        public void BlockSearch()
        {
            lock (objLock)
            {
                IsSearchBlocked = true;
            }
        }

        /// <summary> Sets the database update to be compelted and the search block to unblocked. </summary>
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
