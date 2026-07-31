using NpgsqlTypes;

namespace elite_dangerous_colonise.Models.Database_Types
{
    [PgName("UncolonisedDetailsInsertType")]
    public class UncolonisedDetailsInsertType
    {
        [PgName("systemID")]
        public decimal SystemID { get; set; }
        [PgName("lastUpdated")]
        public DateTime LastUpdate { get; set; }
        [PgName("reserveLevel")]
        public ReserveType ReserveLevel { get; set; }
        [PgName("landableCount")]
        public short LandableCount { get; set; }
        [PgName("walkableCount")]
        public short WalkableCount { get; set; }
        [PgName("totalHotspots")]
        public short TotalHotspots { get; set; }
        [PgName("systemValue")]
        public decimal SystemValue { get; set; }
        [PgName("blackHoleCount")]
        public short BlackHoleCount { get; set; }
        [PgName("neutronStarCount")]
        public short NeutronStarCount { get; set; }
        [PgName("whiteDwarves")]
        public short WhiteDwarves {  get; set; }
        [PgName("otherStarCount")]
        public short OtherStarCount { get; set; }
        [PgName("earthLikeCount")]
        public short EarthLikeCount { get; set; }
        [PgName("waterWorldCount")]
        public short WaterWorldCount { get; set; }
        [PgName("ammoniaWorldCount")]
        public short AmmoniaWorldCount { get; set; }
        [PgName("gasGiantCount")]
        public short GasGiantCount { get; set; }
        [PgName("highMetalContentCount")]
        public short HighMetalContentCount { get; set; }
        [PgName("metalRichCount")]
        public short MetalRichCount { get; set; }
        [PgName("rockyIceBodyCount")]
        public short RockyIceBodyCount { get; set; }
        [PgName("rockBodyCount")]
        public short RockBodyCount { get; set; }
        [PgName("icyBodyCount")]
        public short IcyBodyCount { get; set; }
        [PgName("organicCount")]
        public short OrganicCount { get; set; }
        [PgName("geologicalsCount")]
        public short GeologicalsCount { get; set; }
        [PgName("ringCount")]
        public short RingCount { get; set; }

        public UncolonisedDetailsInsertType() { }
        public UncolonisedDetailsInsertType(
            ulong systemID, DateTime lastUpdate, ReserveType reserveLevel, short landableCount, short walkableCount,
            short totalHotspots, double systemValue, short blackHoleCount, short neutronStarCount, short whiteDwarves, short otherStarCount,
            short earthLikeCount, short waterWorldCount, short ammoniaWorldCount, short gasGiantCount, short highMetalContentCount, short metalRichCount,
            short rockyIceBodyCount, short rockBodyCount, short icyBodyCount, short organicCount, short geologicalsCount, short ringCount
        )
        {
            SystemID = systemID;
            LastUpdate = lastUpdate;
            ReserveLevel = reserveLevel;
            LandableCount = landableCount;
            WalkableCount = walkableCount;
            TotalHotspots = totalHotspots;
            SystemValue = (decimal)systemValue;
            BlackHoleCount = blackHoleCount;
            NeutronStarCount = neutronStarCount;
            WhiteDwarves = whiteDwarves;
            OtherStarCount = otherStarCount;
            EarthLikeCount = earthLikeCount;
            WaterWorldCount = waterWorldCount;
            AmmoniaWorldCount = ammoniaWorldCount;
            GasGiantCount = gasGiantCount;
            HighMetalContentCount = highMetalContentCount;
            MetalRichCount = metalRichCount;
            RockyIceBodyCount = rockyIceBodyCount;
            RockBodyCount = rockBodyCount;
            IcyBodyCount = icyBodyCount;
            OrganicCount = organicCount;
            GeologicalsCount = geologicalsCount;
            RingCount = ringCount;
        }
    }
}
