using elite_dangerous_colonise.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Npgsql;

namespace elite_dangerous_colonise.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StarSystemReportController : ControllerBase
    {
        private const int REPORT_DELAY = 10;
        private readonly NpgsqlDataSource dataSource;
        private readonly AppLogger logger;

        private List<ulong>? reportedStarSystems = null;
        private DateTime? lastReport = null;

        public StarSystemReportController(NpgsqlDataSource dataSource, AppLogger logger)
        {
            this.dataSource = dataSource;
            this.logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] ReportQueryModel reportData)
        {
            ParseSessionValues();

            if (HttpContext.Session.GetString("PassedCaptcha") != "true" || reportedStarSystems == null)
            {
                return StatusCode(401, new
                {
                    success = false,
                    error = "A session could not be found."
                });
            }
            else if (lastReport != null && DateTime.Now - lastReport < TimeSpan.FromSeconds(REPORT_DELAY))
            {
                return StatusCode(429, new
                {
                    success = false,
                    error = "Too many reports within the last 10 seconds."
                });
            }
            else
            {
                try
                {
                    if (!reportedStarSystems.Contains(reportData.ReportedSystemID))
                    {
                        await ReportStarSystem(reportData);
                        AddStarSystemToReportList(reportData.ReportedSystemID);
                    }

                    return Ok();
                }
                catch (NpgsqlException ex)
                {
                    logger.LogError("Star System Report Controller", 0, ex);

                    return StatusCode(500, new
                    {
                        success = false,
                        error = "A database error occurred."
                    });
                }

                catch (Exception ex)
                {
                    logger.LogError("Star System Report Controller", 1, ex);

                    return StatusCode(500, new
                    {
                        success = false,
                        error = "An unexpected error occurred. Please try again later."
                    });
                }
            }
        }

        private void ParseSessionValues()
        {
            string? reportedStarSystemsJson = HttpContext.Session.GetString("reportedStarSystemsJson");
            string? lastReportJson = HttpContext.Session.GetString("lastReportJson");

            if (!string.IsNullOrEmpty(reportedStarSystemsJson))
            {
                reportedStarSystems = JsonConvert.DeserializeObject<List<ulong>>(reportedStarSystemsJson);
            }

            if (!string.IsNullOrEmpty(lastReportJson))
            {
                DateTime lastReport = JsonConvert.DeserializeObject<DateTime>(lastReportJson);
            }
        }

        private async Task ReportStarSystem(ReportQueryModel reportData)
        {
            await using (NpgsqlConnection conn = await dataSource.OpenConnectionAsync())
            {
                await using (NpgsqlCommand command = new NpgsqlCommand("SELECT \"ReportStarSystem\"(@inputSystemID, @isLocked)", conn))
                {
                    command.Parameters.AddWithValue("inputSystemID", (decimal)reportData.ReportedSystemID);
                    command.Parameters.AddWithValue("isLocked", reportData.IsLocked);

                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        private void AddStarSystemToReportList(ulong systemID)
        {
            reportedStarSystems.Add(systemID);
            HttpContext.Session.SetString("reportedStarSystemsJson", JsonConvert.SerializeObject(reportedStarSystems));
        }

        public class ReportQueryModel
        {
            public ulong ReportedSystemID { get; set; }
            public bool IsLocked { get; set; }
        }
    }
}
