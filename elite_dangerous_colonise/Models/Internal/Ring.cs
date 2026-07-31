using elite_dangerous_colonise.Models.Database_Types;

namespace elite_dangerous_colonise.Models.Internal
{
    /// <summary> Contains information about a Ring. </summary>
    public class Ring
    {
        /// <summary> The ring's name. </summary>
        public string Name { get; private set; }
        /// <summary> The ring's material type. </summary>
        public RingType RingType { get; private set; }
        /// <summary> The Hotspots in the ring. </summary>
        public Dictionary<HotspotType, short>? Hotspots { get; private set; }

        /// <summary> Instantiates a Ring. </summary>
        /// <param name="name"> The name of the ring. </param>
        /// <param name="ringType"> The material type of the ring. </param>
        public Ring(string name, RingType ringType)
        {
            Name = name;
            RingType = ringType;
        }
        /// <inheritdoc cref="Ring(string, RingType)"/>
        /// <param name="hotspots"> The hotspots present on the ring. </param>
        public Ring(string name, RingType ringType, Dictionary<HotspotType, short> hotspots) :
            this(name, ringType)
        {
            Hotspots = hotspots;
        }

        private void AddRingToDataList(DatabaseDataLists dataLists, ulong systemID)
        {

            dataLists.Rings.Add( new RingInsertType(systemID, Name, RingType));
        }

        private void AddHotspotToDataList(DatabaseDataLists dataLists, ulong systemID)
        {
            if (Hotspots != null)
            {
                foreach (KeyValuePair<HotspotType, short> hotspot in Hotspots)
                {
                    dataLists.Hotspots.Add(new HotspotInsertType(systemID, Name, hotspot.Key, hotspot.Value));
                }
            }
        }

        /// <summary> Adds the Ring and its Hotspots values to the data lists. </summary>
        public void AddToDataLists(DatabaseDataLists dataLists, ulong systemID)
        {
            AddRingToDataList(dataLists, systemID);
            AddHotspotToDataList(dataLists, systemID);
        }
    }
}
