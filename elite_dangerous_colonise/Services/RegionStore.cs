using elite_dangerous_colonise.Models.Internal;
using Npgsql;
using System.Numerics;

namespace elite_dangerous_colonise.Services
{
    public class RegionStore
    {
        private readonly NpgsqlDataSource dataSource;
        private IReadOnlyList<Region> regions = new List<Region>();
        private IReadOnlyList<Region> activeRegions = new List<Region>();

        public RegionStore(NpgsqlDataSource dataSource)
        {
            this.dataSource = dataSource;
        }

        public IReadOnlyList<Region> GetRegions()
        {
            return regions;
        }

        public IReadOnlyList<Region> GetActiveRegions()
        {
            return activeRegions;
        }

        public async Task Initialise()
        {
            List<Region> regions = new List<Region>();

            await using (NpgsqlConnection conn = await dataSource.OpenConnectionAsync())
            {
                await using (NpgsqlCommand command = new NpgsqlCommand("Select * FROM \"SelectRegionsView\"", conn))
                {
                    await using (NpgsqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            regions.Add(new Region(
                                reader.GetString(0),
                                reader.GetInt32(1),
                                new Vector3(
                                    reader.GetFloat(2),
                                    reader.GetFloat(3),
                                    reader.GetFloat(4)
                                    ),
                                reader.GetString(5),
                                reader.GetBoolean(6)
                            ));
                        }
                    }
                }
            }

            this.regions = regions.AsReadOnly();
            this.activeRegions = regions.Where(region => region.Active).ToList().AsReadOnly();
        }
    }
}
