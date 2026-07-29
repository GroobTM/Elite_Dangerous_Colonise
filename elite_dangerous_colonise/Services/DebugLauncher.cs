namespace elite_dangerous_colonise.Services
{
    public class DebugLauncher
    {
        private readonly IServiceProvider serviceProvider;

        public DebugLauncher(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public async Task Run()
        {
            LauncherHeader();
            await LauncherOptions();
        }

        private static void LauncherHeader()
        {
            Console.WriteLine("-----------------------Elite Dangerous Colonise-----------------------");
            Console.WriteLine("Launch Options:");
            Console.WriteLine("1. Insert galaxy Json data into database.");
            Console.WriteLine("2. Insert Sol Json data into database.");
            Console.WriteLine("3. Insert Colonia Json data into database.");
            Console.WriteLine("4. Exit");
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
                        await InsertIntoDatabase();
                        selectionValid = true;
                        break;

                    case "2":
                        selectionValid = true;
                        break;

                    case "3":
                        selectionValid = true;
                        break;

                    case "4":
                        selectionValid = true;
                        break;

                    default:
                        Console.WriteLine($"{input ?? ""} is not an option.");
                        break;
                }
            }
        }

        private async Task InsertIntoDatabase()
        {
            string? filePath = null;

            while (!(filePath != null && File.Exists(filePath)))
            {
                Console.Write("Enter Json file path: ");
                filePath = Console.ReadLine()?.Trim().Trim('"');
            }

            await using AsyncServiceScope scope = serviceProvider.CreateAsyncScope();
            DatabaseBulkWriter dbWriter = scope.ServiceProvider.GetRequiredService<DatabaseBulkWriter>();

            await dbWriter.InsertJsonIntoDatabase(filePath);
        }
    }
}
