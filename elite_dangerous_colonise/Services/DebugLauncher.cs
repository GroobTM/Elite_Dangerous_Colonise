using System.IO.Compression;
using System.Net.Sockets;

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
            LauncherHeader();
            await LauncherOptions();
        }

        private static void LauncherHeader()
        {
            Console.WriteLine("-----------------------Elite Dangerous Colonise-----------------------");
            Console.WriteLine("Launch Options:");
            Console.WriteLine("1. Insert galaxy Json file into database.");
            Console.WriteLine("2. Insert galaxy GZip file into database.");
            Console.WriteLine("2. Exit");
            Console.WriteLine("----------------------------------------------------------------------");
        }

        private async Task LauncherOptions()
        {
            bool selectionValid = false;

            while (!selectionValid)
            {
                Console.Write("Select an option: ");
                string? input = Console.ReadLine();

                switch (input)
                {
                    case "1":
                        await InsertIntoDatabase(true);
                        selectionValid = true;
                        break;

                    case "2":
                        selectionValid = true;
                        await InsertIntoDatabase(false);
                        break;

                    case "3":
                        selectionValid = true;
                        break;

                    default:
                        Console.WriteLine($"{input ?? ""} is not an option.");
                        break;
                }
            }
        }

        private async Task InsertIntoDatabase(bool readAsJson)
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
    }
}
