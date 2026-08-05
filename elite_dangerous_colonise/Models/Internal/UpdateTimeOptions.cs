namespace elite_dangerous_colonise.Models.Internal
{
    /// <summary> Stores the scheduled database update time. </summary>
    public class UpdateTimeOptions
    {
        /// <summary> The update hour. </summary>
        public int Hour { get; set; } = 0;
        /// <summary> The update minute. </summary>
        public int Minute { get; set; } = 0;
    }
}
