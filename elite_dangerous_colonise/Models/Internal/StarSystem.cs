using elite_dangerous_colonise.Models.Database_Types;
using elite_dangerous_colonise.Services;
using System.Numerics;

namespace elite_dangerous_colonise.Models.Internal
{
    /// <summary> Defines the StarSystem abstract class. </summary>
    public abstract class StarSystem
    {
        public long SystemID { get; private set; }
        public string Name { get; private set; }
        public bool IsColonised { get; private set; }
        public Vector3 Coordinates { get; private set; }

        /// <summary> Instantiates a StarSystem object. </summary>
        /// <param name="systemID"> The system's Spansh ID. </param>
        /// <param name="name"> The name of the system. </param>
        /// <param name="isColonised"> If the system is colonised. </param>
        /// <param name="coordinates"> The system's coordinates. </param>
        public StarSystem(long systemID, string name, bool isColonised, Vector3 coordinates)
        {
            SystemID = systemID;
            Name = name;
            IsColonised = isColonised;
            Coordinates = coordinates;
        }

        /// <summary> Adds the StarSystems to the StarSystems data list. </summary>
        protected void AddSystemToDataList(DatabaseDataLists dataLists)
        {
            dataLists.StarSystems.Add(new StarSystemInsertType(SystemID, Name, IsColonised,
                (decimal)Coordinates.X, (decimal)Coordinates.Y, (decimal)Coordinates.Z));
        }

        /// <summary> Adds the StarSystem and component objects to the datalists. </summary>
        public abstract void AddToDataLists(DatabaseDataLists dataLists);
    }

    /// <summary> Defines the ColonisedStarSystem class. </summary>
    public class ColonisedStarSystem : StarSystem
    {
        public List<Station> Stations { get; private set; }

        /// <summary> Instantiates a ColonisedStarSystem object. </summary>
        /// <inheritdoc cref="StarSystem(long, string, bool, Vector3"/>
        /// <param name="stations"> The list of the system's stations. </param>
        public ColonisedStarSystem(long systemID, string name, Vector3 coordinates, List<Station> stations) :
            base(systemID, name, true, coordinates)
        {
            Stations = stations;
        }

        private void AddStationsToDataList(DatabaseDataLists dataLists)
        {
            if (Stations != null)
            {
                foreach (Station station in Stations)
                {
                    station.AddToDataList(dataLists, SystemID);
                }
            }
        }

        /// <summary> Adds the StarSystem and its Stations to the data list. </summary>
        public override void AddToDataLists(DatabaseDataLists dataLists)
        {
            AddSystemToDataList(dataLists);
            AddStationsToDataList(dataLists);
        }
    }

    /// <summary> Defines the UncolonisedStarSystem class. </summary>
    public class UncolonisedStarSystem : StarSystem
    {
        public DateTime LastUpdate { get; private set; }
        public ReserveType ReserveLevel { get; private set; }
        public short LandableCount { get; private set; }
        public short WalkableCount { get; private set; }
        public int DistanceToSol {  get; private set; }
        public short TotalHotspots { get; private set; }
        public double SystemValue { get; private set; }
        public List<Ring> Rings { get; private set; }
        public BodyCount BodyCounts { get; private set; }

        /// <summary> Instantiates a UncolonisedStarSystem object. </summary>
        /// <inheritdoc cref="StarSystem(long, string, bool, Vector3"/>
        /// <param name="lastUpdate"></param>
        /// <param name="reserveLevel"></param>
        /// <param name="landableCount"></param>
        /// <param name="walkableCount"></param>
        /// <param name="rings"></param>
        public UncolonisedStarSystem(long systemID, string name, Vector3 coordinates, DateTime lastUpdate, ReserveType reserveLevel,
            short landableCount, short walkableCount, List<Ring> rings, BodyCount bodyCounts)
            : base(systemID, name, false, coordinates)
        {
            LastUpdate = lastUpdate;
            ReserveLevel = reserveLevel;
            LandableCount = landableCount;
            WalkableCount = walkableCount;
            Rings = rings;
            BodyCounts = bodyCounts;
            DistanceToSol = (int)coordinates.Length();
            TotalHotspots = CountHotspots();
            SystemValue = CalculateSystemValue();
        }

        private short CountHotspots()
        {
            short count = 0;

            foreach (Ring ring in Rings)
            {
                if (ring.Hotspots != null)
                {
                    count += (short)ring.Hotspots.Values.Sum(value => value);
                }
            }

            return count;
        }

        private double CalculateSystemValue()
        {
            const double WALKABLE_WEIGHT = 0.6;
            const double HOTSPOT_WEIGHT = 0.8;

            return
                BodyCounts.CalculateCountValues() +
                TotalHotspots * HOTSPOT_WEIGHT +
                WalkableCount * WALKABLE_WEIGHT;
        }

        private void AddSystemDetailsToDataLists(DatabaseDataLists dataLists)
        {
            BodyCounts.AddToDataLists(dataLists, this);
        }

        private void AddRingsToDataList(DatabaseDataLists dataLists)
        {
            if (Rings != null)
            {
                foreach (Ring ring in Rings)
                {
                    ring.AddToDataLists(dataLists, SystemID);
                }
            }
        }

        public override void AddToDataLists(DatabaseDataLists dataLists)
        {
            if (SystemValue > 0 && !BodyCounts.IsBoringlyEmpty())
            {
                AddSystemToDataList(dataLists);
                AddSystemDetailsToDataLists(dataLists);
                AddRingsToDataList(dataLists);
            }
        }
    }
}
