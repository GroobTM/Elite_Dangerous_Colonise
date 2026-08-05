using elite_dangerous_colonise.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Npgsql;

namespace elite_dangerous_colonise.Controllers
{
    public abstract class SimpleQueryController : ControllerBase
    {
        private const int MIN_QUERY_LENGTH = 2;
        private const int MAX_QUERY_ATTEMPTS = 3;
        private const int QUERY_ATTEMPT_DELAY = 1;

        protected readonly NpgsqlDataSource dataSource;
        protected readonly AppLogger logger;
        protected readonly string controllerName;

        protected SimpleQueryController(NpgsqlDataSource dataSource, AppLogger logger, string controllerName)
        {
            this.dataSource = dataSource;
            this.logger = logger;
            this.controllerName = controllerName;
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] string? query, [FromQuery] string? region)
        {
            if (string.IsNullOrEmpty(query) || string.IsNullOrEmpty(region) || query.Length < MIN_QUERY_LENGTH)
            {
                return Ok(new JArray());
            }

            try
            {
                for (int attempt = 1; attempt <= MAX_QUERY_ATTEMPTS; attempt++)
                {
                    try
                    {
                        return await ExecuteDatabaseQuery(query, region);
                    }
                    catch (Exception)
                    {
                        if (attempt < MAX_QUERY_ATTEMPTS)
                        {
                            await Task.Delay(TimeSpan.FromSeconds(QUERY_ATTEMPT_DELAY));
                        }
                        else
                        {
                            throw;
                        }
                    }
                }
            }
            catch (NpgsqlException ex)
            {
                logger.LogError(controllerName, 0, ex);

                return StatusCode(500, new
                {
                    success = false,
                    error = "A database error occurred."
                });
            }

            catch (Exception ex)
            {
                logger.LogError(controllerName, 1, ex);

                return StatusCode(500, new
                {
                    success = false,
                    error = "An unexpected error occurred. Please try again later."
                });
            }

            return StatusCode(500, new
            {
                success = false,
                error = "An unexpected error occurred. Please try again later."
            });
        }

        protected abstract Task<IActionResult> ExecuteDatabaseQuery(string query, string region);
    }
}
