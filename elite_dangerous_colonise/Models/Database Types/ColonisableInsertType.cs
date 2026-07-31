using NpgsqlTypes;

namespace elite_dangerous_colonise.Models.Database_Types
{
    [PgName("ColonisableInsertType")]
    public class ColonisableInsertType
    {
        [PgName("colonisedSystemID")]
        public decimal ColonisedSystemID { get; set; }
        [PgName("uncolonisedSystemID")]
        public decimal UncolonisedSystemID { get; set; }

        public ColonisableInsertType() { }
        public ColonisableInsertType(ulong colonisedSystemID, ulong uncolonisedSystemID)
        {
            ColonisedSystemID = colonisedSystemID;
            UncolonisedSystemID = uncolonisedSystemID;
        }
    }
}
