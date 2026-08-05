using elite_dangerous_colonise.Models.Json_Structure;
using System.Numerics;

namespace elite_dangerous_colonise.Models.Internal
{
    /// <summary> Contains information about a Region. </summary>
    public class Region
    {
        /// <summary> The name of the region. </summary>
        public string Name { get; private set; }
        /// <summary> The range of the region. </summary>
        public int Range { get; private set; }
        /// <summary> The centre coordinates of the region. </summary>
        public Vector3 Centre { get; private set; }

        /// <summary> Instantiates a Region. </summary>
        /// <param name="name"> The name of the region. </param>
        /// <param name="range"> The range of the region. </param>
        /// <param name="centre"> The centre coordinates of the region. </param>
        public Region (string name, int range, Vector3 centre)
        {
            Name = name;
            Range = range;
            Centre = centre;
        }

        /// <summary> Checks if a point is within the region. </summary>
        /// <param name="point"> The coordinates of the point to check. </param>
        /// <returns> If the point is within the region. </returns>
        public bool PointWithinRegion(Vector3 point)
        {
            return point.X >= Centre.X - Range
                && point.Y >= Centre.Y - Range
                && point.Z >= Centre.Z - Range
                && point.X <= Centre.X + Range
                && point.Y <= Centre.Y + Range
                && point.Z <= Centre.Z + Range;
        }

        /// <inheritdoc cref="PointWithinRegion(Vector3)"/>
        public bool PointWithinRegion(CoordinatesJson point)
        {
            return point.X >= Centre.X - Range
                && point.Y >= Centre.Y - Range
                && point.Z >= Centre.Z - Range
                && point.X <= Centre.X + Range
                && point.Y <= Centre.Y + Range
                && point.Z <= Centre.Z + Range;
        }
    }
}
