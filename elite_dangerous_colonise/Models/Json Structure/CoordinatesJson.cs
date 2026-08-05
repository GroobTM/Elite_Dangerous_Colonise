using System.Numerics;
using Newtonsoft.Json;

namespace elite_dangerous_colonise.Models.Json_Structure
{
    /// <summary> Represents the Json structure of the system's coordinates. </summary>
    public class CoordinatesJson
    {
        [JsonProperty("x")]
        public required float X { get; set; }
        [JsonProperty("y")]
        public required float Y { get; set; }
        [JsonProperty("z")]
        public required float Z { get; set; }

        /// <summary> Converts the systems coordinates into a vector. </summary>
        /// <returns> A 3D vector of the systems coordinates. </returns>
        public Vector3 ConvertToVector()
        {
            return new Vector3(X, Y, Z);
        }
    }
}
