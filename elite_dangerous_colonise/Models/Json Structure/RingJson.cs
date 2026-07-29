using Newtonsoft.Json;
using elite_dangerous_colonise.Models.Database_Types;
using elite_dangerous_colonise.Models.Internal;

namespace elite_dangerous_colonise.Models.Json_Structure
{
    /// <summary> Defines the Json structure of the body's rings. </summary>
    public class RingJson
    {
        [JsonProperty("name")]
        public required string Name { get; set; }
        [JsonProperty("type")]
        public required string RingType { get; set; }
        [JsonProperty("signals")]
        public SignalsCategoryJson? SignalCategory { get; set; }

        private Ring? ConvertToRing()
        {
            if (Enum.TryParse<RingType>(RingType.Replace(" ", ""), out  RingType ringType))
            {
                if (SignalCategory != null)
                {
                    return new Ring(Name, ringType, SignalCategory.ConvertToHotspots());
                }
                else
                {
                    return new Ring(Name, ringType);
                }
            }
            else
            {
                return null;
            }
        }

        /// <summary> Converts the list of RingJson objects into a list of Ring objects. </summary>
        public static List<Ring> ConvertToRingList(List<RingJson>? rings)
        {
            List<Ring> ringList = new List<Ring>();

            if (rings != null && rings.Count > 0)
            {
                foreach (RingJson ring in rings)
                {
                    Ring? convertedRing = ring.ConvertToRing();

                    if (convertedRing != null)
                    {
                        ringList.Add(convertedRing);
                    }
                }
            }

            return ringList;
        }
    }
}
