using elite_dangerous_colonise.Models.Database_Types;
using System.Numerics;

namespace elite_dangerous_colonise.Models.Internal
{
    /// <summary> Contains information about a Star System. </summary>
    public abstract class StarSystem
    {
        /// <summary> The system's ID.</summary>
        public ulong SystemID { get; private set; }
        /// <summary> The system's name.</summary>
        public string Name { get; private set; }
        /// <summary> The system's colonisation status.</summary>
        public bool IsColonised { get; private set; }
        /// <summary> The controlling faction of the system. </summary>
        public string? ControllingFaction { get; private set; }
        /// <summary> The system's coordinates.</summary>
        public Vector3 Coordinates { get; private set; }

        /// <summary> Instantiates a StarSystem. </summary>
        /// <param name="systemID"> The system's Spansh ID. </param>
        /// <param name="name"> The name of the system. </param>
        /// <param name="isColonised"> If the system is colonised. </param>
        /// <param name="coordinates"> The system's coordinates. </param>
        public StarSystem(ulong systemID, string name, bool isColonised, string? controllingFaction, Vector3 coordinates)
        {
            SystemID = systemID;
            Name = name;
            IsColonised = isColonised;
            ControllingFaction = controllingFaction;
            Coordinates = coordinates;
        }

        /// <summary> Adds the StarSystems to the StarSystems data list. </summary>
        protected void AddSystemToDataList(DatabaseDataLists dataLists)
        {
            dataLists.StarSystems.Add(new StarSystemInsertType(SystemID, Name, IsColonised, ControllingFaction,
                (decimal)Coordinates.X, (decimal)Coordinates.Y, (decimal)Coordinates.Z));
        }

        /// <summary> Adds the StarSystem and component objects to the datalists. </summary>
        public abstract void AddToDataLists(DatabaseDataLists dataLists);
    }

    /// <summary> Contains information about a Colonised Star System. </summary>
    public class ColonisedStarSystem : StarSystem
    {
        /// <summary> Stations in the system. </summary>
        public List<Station> Stations { get; private set; }

        /// <summary> Instantiates a ColonisedStarSystem. </summary>
        /// <inheritdoc cref="StarSystem(ulong, string, bool, Vector3"/>
        /// <param name="stations"> A list of the stations in the system. </param>
        public ColonisedStarSystem(ulong systemID, string name, Vector3 coordinates, string? controllingFaction, List<Station> stations) :
            base(systemID, name, true, controllingFaction, coordinates)
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

        /// <summary> Adds the ColonisedStarSystem and its Stations to the data lists. </summary>
        public override void AddToDataLists(DatabaseDataLists dataLists)
        {
            AddSystemToDataList(dataLists);
            AddStationsToDataList(dataLists);
        }
    }

    /// <summary> Contains information about an Uncolonised Star System. </summary>
    public class UncolonisedStarSystem : StarSystem
    {
        /// <summary> When the system was last updated. </summary>
        public DateTime LastUpdate { get; private set; }
        /// <summary> The system's reserve level. </summary>
        public ReserveType ReserveLevel { get; private set; }
        /// <summary> The number of landable bodies in the system. </summary>
        public short LandableCount { get; private set; }
        /// <summary> The number of walkable bodies in the system. </summary>
        public short WalkableCount { get; private set; }
        /// <summary> The number of hotspots in the system. </summary>
        public short TotalHotspots { get; private set; }
        /// <summary> The system's value. </summary>
        public double SystemValue { get; private set; }
        /// <summary> The rings in the system. </summary>
        public List<Ring> Rings { get; private set; }
        /// <summary> The number of different body types in the system. </summary>
        public BodyCount BodyCounts { get; private set; }

        /// <summary> Instantiates a UncolonisedStarSystem. </summary>
        /// <inheritdoc cref="StarSystem(ulong, string, bool, Vector3"/>
        /// <param name="lastUpdate"> The last time the system was updated. </param>
        /// <param name="reserveLevel"> The reserve level of the system. </param>
        /// <param name="landableCount"> The number of landable bodies in the system. </param>
        /// <param name="walkableCount"> The number of walkable bodies in the system. </param>
        /// <param name="rings"> A list of rings in the system. </param>
        /// <param name="bodyCounts"> The BodyCount for the system. </param>
        public UncolonisedStarSystem(ulong systemID, string name, Vector3 coordinates, DateTime lastUpdate, ReserveType reserveLevel,
            short landableCount, short walkableCount, List<Ring> rings, BodyCount bodyCounts)
            : base(systemID, name, false, null, coordinates)
        {
            LastUpdate = lastUpdate;
            ReserveLevel = reserveLevel;
            LandableCount = landableCount;
            WalkableCount = walkableCount;
            Rings = rings;
            BodyCounts = bodyCounts;
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

        /// <summary> Adds the UncolonisedStarSystem, its details, and its Rings to the data lists. </summary>
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
