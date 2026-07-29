using Newtonsoft.Json;

namespace elite_dangerous_colonise.Models.Json_Structure
{
    /// <summary> Defines the Json structure of the system's bodies. </summary>
    public class BodyJson
    {

        [JsonProperty("id64")]
        public required long BodyID { get; set; }
        [JsonProperty("subType")]
        public required string BodyType { get; set; }
        [JsonProperty("isLandable")]
        public bool IsLandable { get; set; } = false;
        [JsonProperty("reserveLevel")]
        public string? ReserveLevel { get; set; }
        [JsonProperty("gravity")]
        public float? Gravity { get; set; }
        [JsonProperty("surfaceTemperature")]
        public float? SurfaceTemparature { get; set; }
        [JsonProperty("rings")]
        public List<RingJson>? Rings { get; set; }
        [JsonProperty("stations")]
        public List<StationJson>? Stations { get; set; }
        [JsonProperty("signals")]
        public SignalsCategoryJson? SignalCategory { get; set; }

        /// <summary> Checks if the planet can be disembarked on. </summary>
        public bool IsDisembarkable()
        {
            bool isDisembarkable = IsLandable;

            if (Gravity != null && Gravity >= 2.7f)
            {
                isDisembarkable &= false;
            }

            if (SurfaceTemparature != null && SurfaceTemparature >= 800)
            {
                isDisembarkable &= false;
            }

            return isDisembarkable;
        }
    }
}
