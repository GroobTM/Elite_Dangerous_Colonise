using Newtonsoft.Json.Linq;
using elite_dangerous_colonise.Models.Json_Structure;
using System.Numerics;

namespace elite_dangerous_colonise.Services
{
    /// <summary> Static helper methods for calculating the distance between a Star System and Sol (0, 0, 0). </summary>
    public class SolDistanceChecker
    {      
        private const int SOL_COLONY_RANGE = 2000;

        /// <summary> Checks if a set of coordinates in a json file are within colonisation range of Sol. </summary>
        /// <param name="readObject"> The read JObject. </param>
        /// <returns> If the coordinates are within colonisation range of Sol. </returns>
        public static bool InRangeOfSol(JObject readObject)
        {
            JToken coordinates = readObject["coords"];

            if (coordinates != null && coordinates["x"] != null && coordinates["y"] != null && coordinates["z"] != null)
            {
                return Math.Abs(coordinates["x"].Value<float>()) < SOL_COLONY_RANGE
                    && Math.Abs(coordinates["y"].Value<float>()) < SOL_COLONY_RANGE
                    && Math.Abs(coordinates["z"].Value<float>()) < SOL_COLONY_RANGE;
            }
            else
            {
                return false;
            }
        }

        /// <summary> Checks if a set of coordinates are within colonisation range of Sol. </summary>
        /// <param name="coordinates"> A set of coordinates. </param>
        /// <returns> If the coordinates are within colonisation range of Sol. </returns>
        public static bool InRangeOfSol(Vector3 coordinates)
        {
            return Math.Abs(coordinates.X) < SOL_COLONY_RANGE
                && Math.Abs(coordinates.Y) < SOL_COLONY_RANGE
                && Math.Abs(coordinates.Z) < SOL_COLONY_RANGE;
        }

        /// <summary> Checks if a set of coordinates are within colonisation range of Sol. </summary>
        /// <param name="coordinatesJson"> A set of deserialized coordinates. </param>
        /// <returns> If the coordinates are within colonisation range of Sol. </returns>
        public static bool InRangeOfSol(CoordinatesJson coordinatesJson)
        {
            return Math.Abs(coordinatesJson.X) < SOL_COLONY_RANGE
                && Math.Abs(coordinatesJson.Y) < SOL_COLONY_RANGE
                && Math.Abs(coordinatesJson.Z) < SOL_COLONY_RANGE;
        }
    }
}
