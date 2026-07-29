namespace elite_dangerous_colonise.Services
{
    /// <summary> Wrapper service to facilitate easy logging in the correct format. </summary>
    public class AppLogger
    {
        private readonly Serilog.ILogger logger;

        public AppLogger(Serilog.ILogger logger)
        {
            this.logger = logger;
        }

        /// <summary> Creates a information log event. </summary>
        /// <param name="eventCategory"> The category of the event. </param>
        /// <param name="eventID"> The ID of the event. </param>
        /// <param name="eventMessage"> The message shown by the event. </param>
        public void LogInformation(string eventCategory, int eventID, string eventMessage)
        {
            logger.ForContext("SourceContext", eventCategory)
                .Information("[{eventID}] " + eventMessage, eventID);
        }

        /// <summary> Creates an error log event. </summary>
        /// <param name="eventCategory"> The category of the event. </param>
        /// <param name="eventID"> The ID of the event. </param>
        /// <param name="exception"> The exception that triggered the event. </param>
        public void LogError(string eventCategory, int eventID, Exception exception)
        {
            logger.ForContext("SourceContext", eventCategory)
                .Error(exception, "[{eventID}] An error occured:", eventID);
        }

        /// <inheritdoc cref="LogError(string, int, Exception)"/>
        /// <param name="eventMessage"> The message shown by the event. </param>
        public void LogError(string eventCategory, int eventID, string eventMessage, Exception exception)
        {
            logger.ForContext("SourceContext", eventCategory)
                .Error(exception, "[{eventID}] " + eventMessage, eventID);
        }

        /// <summary> Creates a warning log event. </summary>
        /// <param name="eventCategory"> The category of the event. </param>
        /// <param name="eventID"> The ID of the event. </param>
        /// <param name="eventMessage"> The message shown by the event. </param>
        public void LogWarning(string eventCategory, int eventID, string eventMessage)
        {
            logger.ForContext("SourceContext", eventCategory)
                .Warning("[{eventID}] " + eventMessage, eventID);
        }
    }
}
