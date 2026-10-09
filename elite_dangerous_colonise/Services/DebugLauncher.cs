using elite_dangerous_colonise.Models.Internal;
using Npgsql;
using System.Globalization;
using System.IO.Compression;
using System.Numerics;

namespace elite_dangerous_colonise.Services
{
    /// <summary> Creates a launcher for manually inserting a Json file into the database </summary>
    public class DebugLauncher
    {
        private readonly IServiceProvider serviceProvider;

        /// <summary> Instantiates a DebugLauncher. </summary>
        /// <param name="serviceProvider"> A configured service provider with a DatabaseBulkWriter service. </param>
        public DebugLauncher(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        /// <summary> Runs the launcher. </summary>
        public async Task Run()
        {
            bool close = false;

            while (!close)
            {
                LauncherHeader();
                close = await LauncherOptions();
            }

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        private static void LauncherHeader()
        {
            Console.WriteLine("-----------------------Elite Dangerous Colonise-----------------------");
            Console.WriteLine("Launch Options:");
            Console.WriteLine("1. Insert galaxy Json file into database.");
            Console.WriteLine("2. Insert galaxy GZip file into database.");
            Console.WriteLine("3. Insert region into database.");
            Console.WriteLine("4. Toggle region in database.");
            Console.WriteLine("5. Exit");
            Console.WriteLine("----------------------------------------------------------------------");
        }

        private async Task<bool> LauncherOptions()
        {
            bool selectionValid = false;

            while (!selectionValid)
            {
                Console.Write("Select an option: ");
                string? input = Console.ReadLine();

                switch (input)
                {
                    case "1":
                        await InsertSystemsIntoDatabase(true);
                        selectionValid = true;
                        break;

                    case "2":
                        selectionValid = true;
                        await InsertSystemsIntoDatabase(false);
                        break;

                    case "3":
                        selectionValid = true;
                        await InsertRegionIntoDatabase(CreateRegion());
                        break;

                    case "4":
                        selectionValid = true;
                        await ToggleRegionInDatabase();
                        break;

                    case "5":
                        return true;
                }
            }

            return false;
        }

        private async Task InsertSystemsIntoDatabase(bool readAsJson)
        {
            string? filePath = null;

            while (!(filePath != null && File.Exists(filePath)))
            {
                Console.Write("Enter file path: ");
                filePath = Console.ReadLine()?.Trim().Trim('"');
            }

            await using AsyncServiceScope scope = serviceProvider.CreateAsyncScope();
            DatabaseBulkWriter dbWriter = scope.ServiceProvider.GetRequiredService<DatabaseBulkWriter>();

            if (readAsJson)
            {
                await dbWriter.InsertJsonIntoDatabase(filePath);
            }
            else
            {
                using FileStream streamReader = File.OpenRead(filePath);
                await using (GZipStream decompressionStream = new GZipStream(streamReader, CompressionMode.Decompress))
                {
                    await dbWriter.InsertJsonIntoDatabase(decompressionStream);
                }
            }
        }

        private Region CreateRegion()
        {
            Region? region = null;

            while (region == null)
            {
                string? regionName = null;

                while(regionName == null || regionName.Length == 0)
                {
                    Console.Write("Enter region name: ");

                    regionName = Console.ReadLine()?.Trim();
                }

                int? regionRange = null;

                while (regionRange == null || regionRange < 50)
                {
                    Console.Write("Enter region range: ");

                    if (int.TryParse(Console.ReadLine()?.Trim(), out int range))
                    {
                        regionRange = range;
                    }
                }

                Vector3? regionCentre = null;

                while (regionCentre == null)
                {
                    float? centreX = null;
                    float? centreY = null;
                    float? centreZ = null;

                    Console.WriteLine("Enter region centre: ");

                    Console.Write("X: ");

                    if (float.TryParse(Console.ReadLine()?.Trim(), out float x))
                    {
                        centreX = x;
                    }

                    Console.Write("Y: ");

                    if (float.TryParse(Console.ReadLine()?.Trim(), out float y))
                    {
                        centreY = y;
                    }

                    Console.Write("Z: ");

                    if (float.TryParse(Console.ReadLine()?.Trim(), out float z))
                    {
                        centreZ = z;
                    }

                    if (centreX != null && centreY != null && centreZ != null)
                    {
                        regionCentre = new Vector3((float)centreX, (float)centreY, (float)centreZ);
                    }

                    
                }

                string? regionColour = null;

                while (regionColour == null || regionColour.Length != 7)
                {
                    Console.Write("Enter region colour: #");

                    if (int.TryParse(Console.ReadLine()?.Trim(), NumberStyles.HexNumber, null, out int colour))
                    {
                        regionColour = $"#{colour.ToString("X6")}";
                    }
                }

                Console.WriteLine("New Region:");
                Console.WriteLine($"- Name: {regionName}");
                Console.WriteLine($"- Range: {regionRange}");
                Console.WriteLine($"- Centre: {regionCentre}");
                Console.WriteLine($"- Colour: {regionColour}");
                Console.Write("Proceed? (y/n) ");

                string? answer = Console.ReadLine()?.Trim();

                if (answer != null && (answer.ToLower() == "y" || answer.ToLower() == "yes")) {
                    region = new Region(regionName, (int)regionRange, (Vector3)regionCentre, regionColour);
                }
            }

            return region;
        }

        private async Task InsertRegionIntoDatabase(Region region)
        {
            await using AsyncServiceScope scope = serviceProvider.CreateAsyncScope();
            NpgsqlDataSource dataSource = scope.ServiceProvider.GetRequiredService<NpgsqlDataSource>();
            await using (NpgsqlConnection conn = await dataSource.OpenConnectionAsync())
            {
                await using (NpgsqlCommand command = new NpgsqlCommand("SELECT \"InsertRegion\"(@inputRegionName, @inputRegionRange, @inputCoordinateX, @inputCoordinateY, @inputCoordinateZ, @inputRegionColour)", conn))
                {
                    command.Parameters.AddWithValue("inputRegionName", region.Name);
                    command.Parameters.AddWithValue("inputRegionRange", (short)region.Range);
                    command.Parameters.AddWithValue("inputCoordinateX", (decimal)region.Centre.X);
                    command.Parameters.AddWithValue("inputCoordinateY", (decimal)region.Centre.Y);
                    command.Parameters.AddWithValue("inputCoordinateZ", (decimal)region.Centre.Z);
                    command.Parameters.AddWithValue("inputRegionColour", NpgsqlTypes.NpgsqlDbType.Char, region.Colour);

                    await command.ExecuteNonQueryAsync();
                }
            }

            Console.WriteLine($"Region \"{region.Name}\" added to database.");
        }

        private async Task ToggleRegionInDatabase()
        {
            string? regionName = null;

            while (!(regionName != null))
            {
                Console.Write("Enter region name: ");
                regionName = Console.ReadLine()?.Trim();
            }

            await using AsyncServiceScope scope = serviceProvider.CreateAsyncScope();
            NpgsqlDataSource dataSource = scope.ServiceProvider.GetRequiredService<NpgsqlDataSource>();
            await using (NpgsqlConnection conn = await dataSource.OpenConnectionAsync())
            {
                await using (NpgsqlCommand command = new NpgsqlCommand("UPDATE \"Regions\" SET \"regionActive\" = NOT \"regionActive\" WHERE \"regionName\" = @regionName RETURNING \"regionActive\"", conn))
                {
                    command.Parameters.AddWithValue("regionName", regionName);

                    await using (NpgsqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        if (reader.Read())
                        {
                            Console.WriteLine($"Region \"{regionName}\" was set to {(reader.GetBoolean(0) ? "" : "not ")}active.");
                        }
                        else
                        {
                            Console.WriteLine($"Region \"{regionName}\" not found.");
                        }
                    }
                }
            }
        }
    }
}
