using elite_dangerous_colonise.Models.Database_Types;
using elite_dangerous_colonise.Services;

namespace elite_dangerous_colonise.Models.Internal
{
    /// <summary> Defines a ring. </summary>
    public class Ring
    {
        public string Name { get; private set; }
        public RingType RingType { get; private set; }
        public Dictionary<HotspotType, short>? Hotspots { get; private set; }

        /// <summary> Instantiates a ring object. </summary>
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

        private void AddRingToDataList(DatabaseDataLists dataLists, long systemID)
        {

            dataLists.Rings.Add( new RingInsertType(systemID, Name, RingType));
        }

        private void AddHotspotToDataList(DatabaseDataLists dataLists, long systemID)
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
        public void AddToDataLists(DatabaseDataLists dataLists, long systemID)
        {
            AddRingToDataList(dataLists, systemID);
            AddHotspotToDataList(dataLists, systemID);
        }
    }
}
