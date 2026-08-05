using NpgsqlTypes;

namespace elite_dangerous_colonise.Models.Database_Types
{
    [PgName("ResultOrderType")]
    public enum ResultOrderType
    {
        [PgName("SystemValue")]
        SystemValue,
        [PgName("MostWalkables")]
        MostWalkables,
        [PgName("DistanceToRegionCentre")]
        DistanceToRegionCentre,
        [PgName("MostHotspots")]
        MostHotspots
    }
}
