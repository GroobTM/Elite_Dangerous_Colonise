using elite_dangerous_colonise.Models.Database_Types;

namespace elite_dangerous_colonise.Models.Internal
{
    /// <summary> Contains information about the number of different body types in a StarSystem. </summary>
    public class BodyCount
    {
        private const double INTERESTING_WEIGHT = 1.0;
        private const double MEH_WEIGHT = 0.5;
        private const double BORING_WEIGHT = 0.2;

        /// <summary> The number of black holes in the system. </summary>
        public short BlackHoleCount { get; set; } = 0;
        /// <summary> The number of neutron stars in the system. </summary>
        public short NeutronStarCount { get; set; } = 0;
        /// <summary> The number of white dwarves in the system. </summary>
        public short WhiteDwarves { get; set; } = 0;
        /// <summary> The number of other star types in the system. </summary>
        public short OtherStarCount { get; set; } = 0;
        /// <summary> The number of Earth-like worlds in the system. </summary>
        public short EarthLikeCount { get; set; } = 0;
        /// <summary> The number of water worlds in the system. </summary>
        public short WaterWorldCount { get; set; } = 0;
        /// <summary> The number of ammonia world in the system. </summary>
        public short AmmoniaWorldCount { get; set; } = 0;
        /// <summary> The number of gas giants in the system. </summary>
        public short GasGiantCount { get; set; } = 0;
        /// <summary> The number of high metal content bodies in the system. </summary>
        public short HighMetalContentCount { get; set; } = 0;
        /// <summary> The number of metal rich bodies in the system. </summary>
        public short MetalRichCount { get; set; } = 0;
        /// <summary> The number of rocky icy bodies in the system. </summary>
        public short RockyIceBodyCount { get; set; } = 0;
        /// <summary> The number of rocky bodies in the system. </summary>
        public short RockBodyCount { get; set; } = 0;
        /// <summary> The number of icy bodies in the system. </summary>
        public short IcyBodyCount { get; set; } = 0;
        /// <summary> The number of organics in the system. </summary>
        public short OrganicCount { get; set; } = 0;
        /// <summary> The number of geologicals in the system. </summary>
        public short GeologicalsCount { get; set; } = 0;
        /// <summary> The number of rings in the system. </summary>
        public short RingCount { get; set; } = 0;
        /// <summary> The number of terraformable bodies in the system. </summary>
        public short TerraformableCount { get; set; } = 0;
        /// <summary> The number of bodies with volcanism. </summary>
        public short VolcanismCount { get; set; } = 0;

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
                RingCount * MEH_WEIGHT +
                TerraformableCount * INTERESTING_WEIGHT +
                VolcanismCount * MEH_WEIGHT;
        }

        /// <summary> Adds the BodyCount to the UncolonisedDetails data list. </summary>
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
                    RingCount,
                    TerraformableCount,
                    VolcanismCount
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
                && RingCount == 0
                && TerraformableCount == 0
                && VolcanismCount == 0;
        }
    }
}
