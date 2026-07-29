using elite_dangerous_colonise.Models.Database_Types;
using Newtonsoft.Json;

namespace elite_dangerous_colonise.Models.Json_Structure
{
    /// <summary> Represents the Json structure of the ring's signal categories. </summary>
    public class SignalsCategoryJson
    {
        [JsonProperty("signals")]
        public required Dictionary<string, short> SignalTypes { get; set; }

        /// <summary> Converts the dictionary of signals into a dictionary of hotspot types. </summary>
        public Dictionary<HotspotType, short> ConvertToHotspots()
        {
            Dictionary<HotspotType, short> hotspots = new Dictionary<HotspotType, short>();

            foreach (KeyValuePair<string, short> signal  in SignalTypes)
            {
                if (Enum.TryParse<HotspotType>(signal.Key, out HotspotType hotspotType))
                {
                    hotspots.Add(hotspotType, signal.Value);
                }
            }

            return hotspots;
        }
    }
}
