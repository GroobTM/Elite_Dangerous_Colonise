using elite_dangerous_colonise.Models.Database_Types;

namespace elite_dangerous_colonise.Models.Internal
{
    /// <summary> Holds lists of bulk input data to be added to the database. </summary>
    public class DatabaseDataLists
    {
        /// <summary> A list of Star Systems to insert into the database. </summary>
        public List<StarSystemInsertType> StarSystems { get; set; } = new List<StarSystemInsertType>();
        /// <summary> A list of Stations to insert into the database. </summary>
        public List<StationInsertType> Stations { get; set; } = new List<StationInsertType>();
        /// <summary> A list of Rings to insert into the database. </summary>
        public List<RingInsertType> Rings { get; set; } = new List<RingInsertType>();
        /// <summary> A list of Hotspots to insert into the database. </summary>
        public List<HotspotInsertType> Hotspots { get; set; } = new List<HotspotInsertType>();
        /// <summary> A list of system details to insert into the database. </summary>
        public List<UncolonisedDetailsInsertType> UncolonisedDetails { get; set; } = new List<UncolonisedDetailsInsertType>();

        /// <summary> Clears all lists. </summary>
        public void ClearLists()
        {
            StarSystems.Clear();
            Stations.Clear();
            Rings.Clear();
            Hotspots.Clear();
            UncolonisedDetails.Clear();
        }

        /// <summary> Counts the number of Star Systems in the list. </summary>
        public int Count()
        {
            return StarSystems.Count;
        }

        /// <summary> Gets the names of the Star Systems in the list. </summary>
        public List<string> GetNames()
        {
            return StarSystems.Select(value => value.SystemName).ToList();
        }

        /// <summary> Removes duplicate entries from all lists. </summary>
        public void Deduplicate()
        {
            StarSystems = StarSystems.GroupBy(ss => new { ss.SystemID }).Select(g => g.First()).ToList();
            Stations = Stations.GroupBy(s => new { s.StationID, s.SystemID }).Select(g => g.First()).ToList();
            Rings = Rings.GroupBy(r => new { r.SystemID, r.RingName }).Select(g => g.First()).ToList();
            Hotspots = Hotspots.GroupBy(h => new { h.SystemID, h.RingName, h.Type }).Select(g => g.First()).ToList();
            UncolonisedDetails = UncolonisedDetails.GroupBy(ud => new { ud.SystemID }).Select(g => g.First()).ToList();
        }
    }
}
