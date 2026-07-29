using elite_dangerous_colonise.Models.Database_Types;
using elite_dangerous_colonise.Services;

namespace elite_dangerous_colonise.Models.Internal
{
    /// <summary> Defines the BodyCount object. </summary>
    public class BodyCount
    {
        private const double INTERESTING_WEIGHT = 1.0;
        private const double MEH_WEIGHT = 0.5;
        private const double BORING_WEIGHT = 0.2;

        public short BlackHoleCount { get; set; } = 0;
        public short NeutronStarCount { get; set; } = 0;
        public short WhiteDwarves { get; set; } = 0;
        public short OtherStarCount { get; set; } = 0;
        public short EarthLikeCount { get; set; } = 0;
        public short WaterWorldCount { get; set; } = 0;
        public short AmmoniaWorldCount { get; set; } = 0;
        public short GasGiantCount { get; set; } = 0;
        public short HighMetalContentCount { get; set; } = 0;
        public short MetalRichCount { get; set; } = 0;
        public short RockyIceBodyCount { get; set; } = 0;
        public short RockBodyCount { get; set; } = 0;
        public short IcyBodyCount { get; set; } = 0;
        public short OrganicCount { get; set; } = 0;
        public short GeologicalsCount { get; set; } = 0;
        public short RingCount { get; set; } = 0;

        /// <summary> Increases the counter of the corresponding body type.</summary>
        public void BinBodyTypes(string bodyType)
        {
            if (bodyType != null)
            {
                if (bodyType == "Black Hole")
                {
                    BlackHoleCount++;
                }
                else if (bodyType == "Neutron Star")
                {
                    NeutronStarCount++;
                }
                else if (bodyType.Contains("White Dwarf"))
                {
                    WhiteDwarves++;
                }
                else if (bodyType.Contains("Star"))
                {
                    OtherStarCount++;
                }
                else if (bodyType == "Earth-like world")
                {
                    EarthLikeCount++;
                }
                else if (bodyType == "Water world")
                {
                    WaterWorldCount++;
                }
                else if (bodyType == "Ammonia world")
                {
                    AmmoniaWorldCount++;
                }
                else if (bodyType.Contains("giant"))
                {
                    GasGiantCount++;
                }
                else if (bodyType == "High metal content world")
                {
                    HighMetalContentCount++;
                }
                else if (bodyType == "Metal-rich body")
                {
                    MetalRichCount++;
                }
                else if (bodyType == "Rocky Ice world")
                {
                    RockyIceBodyCount++;
                }
                else if (bodyType == "Rocky body")
                {
                    RockBodyCount++;
                }
                else if (bodyType == "Icy body")
                {
                    IcyBodyCount++;
                }
            }
        }

        /// <summary> Calculates the system value based on the weighted sum of the body counts. </summary>
        public double CalculateCountValues()
        {
            return 
                BlackHoleCount * INTERESTING_WEIGHT +
                NeutronStarCount * INTERESTING_WEIGHT +
                WhiteDwarves * INTERESTING_WEIGHT +
                OtherStarCount * BORING_WEIGHT +
                EarthLikeCount * INTERESTING_WEIGHT +
                WaterWorldCount * INTERESTING_WEIGHT +
                AmmoniaWorldCount * INTERESTING_WEIGHT +
                GasGiantCount * BORING_WEIGHT +
                HighMetalContentCount * BORING_WEIGHT +
                MetalRichCount * BORING_WEIGHT +
                RockyIceBodyCount * MEH_WEIGHT +
                RockBodyCount * MEH_WEIGHT +
                IcyBodyCount * BORING_WEIGHT +
                OrganicCount * INTERESTING_WEIGHT +
                GeologicalsCount * INTERESTING_WEIGHT +
                RingCount * MEH_WEIGHT;
        }

        /// <summary> Adds the BodyCount to the data lists. </summary>
        /// <param name="starSystem"> The system that has the BodyCount object. </param>
        public void AddToDataLists(DatabaseDataLists dataLists, UncolonisedStarSystem starSystem)
        {
            dataLists.UncolonisedDetails.Add(
                new UncolonisedDetailsInsertType(
                    starSystem.SystemID,
                    starSystem.LastUpdate,
                    starSystem.ReserveLevel,
                    starSystem.LandableCount,
                    starSystem.WalkableCount,
                    starSystem.DistanceToSol,
                    starSystem.TotalHotspots,
                    starSystem.SystemValue,
                    BlackHoleCount,
                    NeutronStarCount,
                    WhiteDwarves,
                    OtherStarCount,
                    EarthLikeCount,
                    WaterWorldCount,
                    AmmoniaWorldCount,
                    GasGiantCount,
                    HighMetalContentCount,
                    MetalRichCount,
                    RockyIceBodyCount,
                    RockBodyCount,
                    IcyBodyCount,
                    OrganicCount,
                    GeologicalsCount,
                    RingCount
                )
            );
        }

        /// <summary> Checks if the system has no bodies and less than or equal to 5 boring stars. </summary>
        public bool IsBoringlyEmpty()
        {
            return
                BlackHoleCount == 0
                && NeutronStarCount == 0
                && WhiteDwarves == 0
                && OtherStarCount <= 5
                && EarthLikeCount == 0
                && WaterWorldCount == 0
                && AmmoniaWorldCount == 0
                && GasGiantCount == 0
                && HighMetalContentCount == 0
                && MetalRichCount == 0
                && RockyIceBodyCount == 0
                && RockBodyCount == 0
                && IcyBodyCount == 0
                && OrganicCount == 0
                && GeologicalsCount == 0
                && RingCount == 0;
        }
    }
}
