using elite_dangerous_colonise.Models.Database_Types;
using elite_dangerous_colonise.Services;

namespace elite_dangerous_colonise.Models.Internal
{
    /// <summary> Defines a station. </summary>
    public class Station
    {
        public ulong StationID { get; private set; }
        public string Name { get; private set; }
        public string Faction { get; private set; }

        /// <summary>
        /// Instantiates a station object.
        /// </summary>
        /// <param name="stationID"> The Spansh ID of the station. </param>
        /// <param name="name"> The name of the station. </param>
        /// <param name="faction"> The station's controlling faction. </param>
        public Station(ulong stationID, string name, string faction)
        {
            StationID = stationID;
            Name = name;
            Faction = faction;
        }

        /// <summary>
        /// Adds the Station to the data lists.
        /// </summary>
        public void AddToDataList(DatabaseDataLists dataLists, long systemID)
        {
            dataLists.Stations.Add(new StationInsertType(StationID, systemID, Name, Faction != null ? Faction : "None"));
        }
    }
}
