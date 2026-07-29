using elite_dangerous_colonise.Classes;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Npgsql;

namespace elite_dangerous_colonise.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ColonisedSystemNamesController : SimpleQueryController
    {
        public ColonisedSystemNamesController(NpgsqlDataSource dataSource, AppLogger logger)
            : base(dataSource, logger, "Colonised System Names Controller") { }

        protected override async Task<IActionResult> ExecuteDatabaseQuery(string query)
        {
            await using (NpgsqlConnection conn = await dataSource.OpenConnectionAsync())
            {
                await using (NpgsqlCommand command = new NpgsqlCommand("SELECT \"SelectColonisedSystemNamesJson\"(@name)", conn))
                {
                    command.Parameters.AddWithValue("name", query);

                    await using (NpgsqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync() && !await reader.IsDBNullAsync(0))
                        {
                            string jsonResult = reader.GetFieldValue<string>(0);

                            JArray parsedJson = JArray.Parse(jsonResult);

                            return Content(parsedJson.ToString(), "application/json");
                        }
                        else
                        {
                            return Content("[]", "application/json");
                        }
                    }
                }
            }
        }
    }
}
