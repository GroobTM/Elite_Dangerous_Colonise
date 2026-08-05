using elite_dangerous_colonise.Models.Database_Types;

namespace elite_dangerous_colonise.Models.Internal
{
    /// <summary> Contains information about a Station. </summary>
    public class Station
    {
        /// <summary> The stations ID. </summary>
        public ulong StationID { get; private set; }
        /// <summary> The station's name. </summary>
        public string Name { get; private set; }
        /// <summary> The station's controlling faction. </summary>
        public string Faction { get; private set; }

        /// <summary> Instantiates a Station. </summary>
        /// <param name="stationID"> The Spansh ID of the station. </param>
        /// <param name="name"> The name of the station. </param>
        /// <param name="faction"> The station's controlling faction. </param>
        public Station(ulong stationID, string name, string faction)
        {
            StationID = stationID;
            Name = name;
            Faction = faction;
        }

        /// <summary> Adds the Station to the Stations data list. </summary>
        public void AddToDataList(DatabaseDataLists dataLists, ulong systemID)
        {
            dataLists.Stations.Add(new StationInsertType(StationID, systemID, Name, Faction != null ? Faction : "None"));
        }
    }
}
