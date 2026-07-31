using NpgsqlTypes;

namespace elite_dangerous_colonise.Models.Database_Types
{
    [PgName("StarSystemInsertType")]
    public class StarSystemInsertType
    {
        [PgName("systemID")]
        public decimal SystemID { get; set; }
        [PgName("systemName")]
        public string SystemName { get; set; }
        [PgName("isColonised")]
        public bool IsColonised { get; set; }
        [PgName("coordinateX")]
        public decimal CoordinateX { get; set; }
        [PgName("coordinateY")]
        public decimal CoordinateY { get; set; }
        [PgName("coordinateZ")]
        public decimal CoordinateZ { get; set; }

        public StarSystemInsertType() { }
        public StarSystemInsertType(ulong systemID, string systemName, bool isColonised, decimal coordinateX, decimal coordinateY, decimal coordinateZ)
        {
            SystemID = systemID;
            SystemName = systemName;
            IsColonised = isColonised;
            CoordinateX = coordinateX;
            CoordinateY = coordinateY;
            CoordinateZ = coordinateZ;
        }
    }
}
