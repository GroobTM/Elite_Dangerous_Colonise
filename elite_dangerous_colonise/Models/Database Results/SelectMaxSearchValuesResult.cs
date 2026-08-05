namespace elite_dangerous_colonise.Models.Database_Results
{
    public class SelectMaxSearchValuesResult
    {
        public int LandableCount { get; private set; }
        public int WalkableCount { get; private set; }
        public int DistanceToRegionCentre { get; private set; }
        public int TotalHotspots { get; private set; }
        public int BlackHoleCount { get; private set; }
        public int NeutronStarCount { get; private set; }
        public int WhiteDwarves { get; private set; }
        public int OtherStarCount { get; private set; }
        public int EarthLikeCount { get; private set; }
        public int WaterWorldCount { get; private set; }
        public int AmmoniaWorldCount { get; private set; }
        public int GasGiantCount { get; private set; }
        public int HighMetalContentCount { get; private set; }
        public int MetalRichCount { get; private set; }
        public int RockyIceBodyCount { get; private set; }
        public int RockBodyCount { get; private set; }
        public int IcyBodyCount { get; private set; }
        public int OrganicCount { get; private set; }
        public int GeologicalsCount { get; private set; }
        public int RingCount { get; private set; }

        public SelectMaxSearchValuesResult(int landableCount, int walkableCount, int distanceToRegionCentre, int totalHotspots, int blackHoleCount,
            int neutronStarCount, int whiteDwarves, int otherStarCount, int earthLikeCount, int waterWorldCount, int ammoniaWorldCount,
            int gasGiantCount, int highMetalContentCount, int metalRichCount, int rockyIceBodyCount, int rockBodyCount, int icyBodyCount,
            int organicCount, int geologicalsCount, int ringCount)
        {
            LandableCount = landableCount;
            WalkableCount = walkableCount;
            DistanceToRegionCentre = distanceToRegionCentre;
            TotalHotspots = totalHotspots;
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
