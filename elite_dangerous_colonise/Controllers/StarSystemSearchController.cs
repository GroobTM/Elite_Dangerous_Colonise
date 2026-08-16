using elite_dangerous_colonise.Models.Database_Types;
using elite_dangerous_colonise.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Npgsql;
using NpgsqlTypes;

namespace elite_dangerous_colonise.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StarSystemSearchController : ControllerBase
    {
        private const int MAX_QUERY_ATTEMPTS = 3;
        private const int QUERY_ATTEMPT_DELAY = 1;

        private readonly NpgsqlDataSource dataSource;
        private readonly UpdateStatusService updateStatusService;
        private readonly AppLogger logger;

        public StarSystemSearchController(NpgsqlDataSource dataSource, UpdateStatusService updateStatusService, AppLogger logger)
        {
            this.dataSource = dataSource;
            this.updateStatusService = updateStatusService;
            this.logger = logger;
        }


        private ResultOrderType ParseSortOrder(string inputOrder)
        {
            if (Enum.TryParse<ResultOrderType>(inputOrder, out ResultOrderType resultOrder))
            {
                return resultOrder;
            }
            else
            {
                return ResultOrderType.SystemValue;
            }
        }

        private List<HotspotType>? ParseHotspotTypes(string? inputHotspots)
        {
            List<HotspotType> hotspotTypes = new List<HotspotType>();

            if (inputHotspots != null)
            {
                foreach (string hotspot in inputHotspots.Split(","))
                {
                    if (Enum.TryParse<HotspotType>(hotspot, out HotspotType hotspotType))
                    {
                        hotspotTypes.Add(hotspotType);
                    }
                }
            }

            if (hotspotTypes.Count > 0)
            {
                return hotspotTypes;
            }
            else
            {
                return null;
            }
        }

        private List<decimal> ParseSessionReportedStarSystems()
        {
            List<decimal> reportedStarSystems = new List<decimal>();
            string? reportedStarSystemsJson = HttpContext.Session.GetString("reportedStarSystemsJson");

            if (!string.IsNullOrEmpty(reportedStarSystemsJson))
            {
                reportedStarSystems = JsonConvert.DeserializeObject<List<decimal>>(reportedStarSystemsJson);
            }

            return reportedStarSystems;
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] SearchQueryModel searchQuery)
        {
            if (!updateStatusService.IsSearchBlocked)
            {
                try
                {
                    for (int attempt = 1; attempt <= MAX_QUERY_ATTEMPTS; attempt++)
                    {
                        try
                        {
                            await using (NpgsqlConnection conn = await dataSource.OpenConnectionAsync())
                            {
                                await using (NpgsqlCommand command = new NpgsqlCommand("SELECT \"SelectSearchResults\"(" +
                                    "@regionName, " +
                                    "@sortOrder, " +
                                    "@pageNo, " +
                                    "@resultsPerPage, " +
                                    "@systemName, " +
                                    "@systemNameRange," +
                                    "@factionSearchMode," +
                                    "@factionName, " +
                                    "@minBlackHoles, " +
                                    "@maxBlackHoles, " +
                                    "@minNeutronStars, " +
                                    "@maxNeutronStars, " +
                                    "@minWhiteDwarves, " +
                                    "@maxWhiteDwarves, " +
                                    "@minOtherStars, " +
                                    "@maxOtherStars, " +
                                    "@minEarthLikes, " +
                                    "@maxEarthLikes, " +
                                    "@minWaterWorlds, " +
                                    "@maxWaterWorlds, " +
                                    "@minAmmoniaWorlds, " +
                                    "@maxAmmoniaWorlds, " +
                                    "@minGasGiants, " +
                                    "@maxGasGiants, " +
                                    "@minHighMetalContents, " +
                                    "@maxHighMetalContents, " +
                                    "@minMetalRiches, " +
                                    "@maxMetalRiches, " +
                                    "@minRockyIces, " +
                                    "@maxRockyIces, " +
                                    "@minRocks, " +
                                    "@maxRocks, " +
                                    "@minIces, " +
                                    "@maxIces, " +
                                    "@minOrganics, " +
                                    "@maxOrganics, " +
                                    "@minGeologicals, " +
                                    "@maxGeologicals, " +
                                    "@minRings, " +
                                    "@maxRings, " +
                                    "@minLandables, " +
                                    "@maxLandables, " +
                                    "@minWalkables, " +
                                    "@maxWalkables, " +
                                    "@minTerraformables," +
                                    "@maxTerraformables," +
                                    "@minVolcanics," +
                                    "@maxVolcanics," +
                                    "@maxDistanceToSol, " +
                                    "@hotspotTypes," +
                                    "@removedSystemIDs" +
                                    ")", conn))
                                {
                                    command.Parameters.AddWithValue("regionName", searchQuery.RegionName);
                                    command.Parameters.AddWithValue("sortOrder", ParseSortOrder(searchQuery.SortOrder));
                                    command.Parameters.AddWithValue("pageNo", Math.Max(1, searchQuery.PageNo));
                                    command.Parameters.AddWithValue("resultsPerPage", Math.Min((short)50, searchQuery.ResultsPerPage));
                                    command.Parameters.AddWithValue("systemName", NpgsqlDbType.Varchar, (object?)searchQuery.SystemName ?? DBNull.Value);
                                    command.Parameters.AddWithValue("systemNameRange", Math.Min(Math.Max((short)0, searchQuery.MaxDistanceToReferenceSystem), (short)100));
                                    command.Parameters.AddWithValue("factionSearchMode", searchQuery.FactionSearchMode);
                                    command.Parameters.AddWithValue("factionName", NpgsqlDbType.Varchar, (object?)searchQuery.FactionName ?? DBNull.Value);
                                    command.Parameters.AddWithValue("minBlackHoles", searchQuery.MinBlackHoles);
                                    command.Parameters.AddWithValue("maxBlackHoles", searchQuery.MaxBlackHoles);
                                    command.Parameters.AddWithValue("minNeutronStars", searchQuery.MinNeutronStars);
                                    command.Parameters.AddWithValue("maxNeutronStars", searchQuery.MaxNeutronStars);
                                    command.Parameters.AddWithValue("minWhiteDwarves", searchQuery.MinWhiteDwarves);
                                    command.Parameters.AddWithValue("maxWhiteDwarves", searchQuery.MaxWhiteDwarves);
                                    command.Parameters.AddWithValue("minOtherStars", searchQuery.MinOtherStars);
                                    command.Parameters.AddWithValue("maxOtherStars", searchQuery.MaxOtherStars);
                                    command.Parameters.AddWithValue("minEarthLikes", searchQuery.MinEarthLikes);
                                    command.Parameters.AddWithValue("maxEarthLikes", searchQuery.MaxEarthLikes);
                                    command.Parameters.AddWithValue("minWaterWorlds", searchQuery.MinWaterWorlds);
                                    command.Parameters.AddWithValue("maxWaterWorlds", searchQuery.MaxWaterWorlds);
                                    command.Parameters.AddWithValue("minAmmoniaWorlds", searchQuery.MinAmmoniaWorlds);
                                    command.Parameters.AddWithValue("maxAmmoniaWorlds", searchQuery.MaxAmmoniaWorlds);
                                    command.Parameters.AddWithValue("minGasGiants", searchQuery.MinGasGiants);
                                    command.Parameters.AddWithValue("maxGasGiants", searchQuery.MaxGasGiants);
                                    command.Parameters.AddWithValue("minHighMetalContents", searchQuery.MinHighMetalContents);
                                    command.Parameters.AddWithValue("maxHighMetalContents", searchQuery.MaxHighMetalContents);
                                    command.Parameters.AddWithValue("minMetalRiches", searchQuery.MinMetalRiches);
                                    command.Parameters.AddWithValue("maxMetalRiches", searchQuery.MaxMetalRiches);
                                    command.Parameters.AddWithValue("minRockyIces", searchQuery.MinRockyIces);
                                    command.Parameters.AddWithValue("maxRockyIces", searchQuery.MaxRockyIces);
                                    command.Parameters.AddWithValue("minRocks", searchQuery.MinRocks);
                                    command.Parameters.AddWithValue("maxRocks", searchQuery.MaxRocks);
                                    command.Parameters.AddWithValue("minIces", searchQuery.MinIces);
                                    command.Parameters.AddWithValue("maxIces", searchQuery.MaxIces);
                                    command.Parameters.AddWithValue("minOrganics", searchQuery.MinOrganics);
                                    command.Parameters.AddWithValue("maxOrganics", searchQuery.MaxOrganics);
                                    command.Parameters.AddWithValue("minGeologicals", searchQuery.MinGeologicals);
                                    command.Parameters.AddWithValue("maxGeologicals", searchQuery.MaxGeologicals);
                                    command.Parameters.AddWithValue("minRings", searchQuery.MinRings);
                                    command.Parameters.AddWithValue("maxRings", searchQuery.MaxRings);
                                    command.Parameters.AddWithValue("minLandables", searchQuery.MinLandables);
                                    command.Parameters.AddWithValue("maxLandables", searchQuery.MaxLandables);
                                    command.Parameters.AddWithValue("minWalkables", searchQuery.MinWalkables);
                                    command.Parameters.AddWithValue("maxWalkables", searchQuery.MaxWalkables);
                                    command.Parameters.AddWithValue("minTerraformables", searchQuery.MinTerraformables);
                                    command.Parameters.AddWithValue("maxTerraformables", searchQuery.MaxTerraformables);
                                    command.Parameters.AddWithValue("minVolcanics", searchQuery.MinVolcanics);
                                    command.Parameters.AddWithValue("maxVolcanics", searchQuery.MaxVolcanics);
                                    command.Parameters.AddWithValue("maxDistanceToSol", searchQuery.MaxDistanceToRegionCentre);
                                    command.Parameters.AddWithValue("hotspotTypes", (object?)ParseHotspotTypes(searchQuery.HotspotTypes) ?? DBNull.Value);
                                    command.Parameters.AddWithValue("removedSystemIDs", ParseSessionReportedStarSystems());

                                    await using (NpgsqlDataReader reader = await command.ExecuteReaderAsync())
                                    {
                                        if (await reader.ReadAsync() && !await reader.IsDBNullAsync(0))
                                        {
                                            string jsonResult = reader.GetFieldValue<string>(0);

                                            JObject parsedJson = JObject.Parse(jsonResult);

                                            return Content(parsedJson.ToString(), "application/json");
                                        }
                                        else
                                        {
                                            return Content("{}", "application/json");
                                        }
                                    }
                                }
                            }
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
                    logger.LogError("Star System Search Controller", 0, ex);

                    return StatusCode(500, new
                    {
                        success = false,
                        error = "A database error occurred."
                    });
                }

                catch (Exception ex)
                {
                    logger.LogError("Star System Search Controller", 1, ex);

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
            else
            {
                return StatusCode(403, new
                {
                    success = false,
                    error = "Search is currently blocked."
                });
            }
        }

        public class SearchQueryModel
        {
            public required string RegionName { get; set; }
            public required string SortOrder { get; set; }
            public int PageNo { get; set; }
            public short ResultsPerPage { get; set; }
            public string? SystemName { get; set; }
            public short MaxDistanceToReferenceSystem { get; set; }
            public string? FactionName { get; set; }
            public bool FactionSearchMode { get; set; }
            public short MinBlackHoles { get; set; }
            public short MaxBlackHoles { get; set; }
            public short MinNeutronStars { get; set; }
            public short MaxNeutronStars { get; set; }
            public short MinWhiteDwarves { get; set; }
            public short MaxWhiteDwarves { get; set; }
            public short MinOtherStars { get; set; }
            public short MaxOtherStars { get; set; }
            public short MinEarthLikes { get; set; }
            public short MaxEarthLikes { get; set; }
            public short MinWaterWorlds { get; set; }
            public short MaxWaterWorlds { get; set; }
            public short MinAmmoniaWorlds { get; set; }
            public short MaxAmmoniaWorlds { get; set; }
            public short MinGasGiants { get; set; }
            public short MaxGasGiants { get; set; }
            public short MinHighMetalContents { get; set; }
            public short MaxHighMetalContents { get; set; }
            public short MinMetalRiches { get; set; }
            public short MaxMetalRiches { get; set; }
            public short MinRockyIces { get; set; }
            public short MaxRockyIces { get; set; }
            public short MinRocks { get; set; }
            public short MaxRocks { get; set; }
            public short MinIces { get; set; }
            public short MaxIces { get; set; }
            public short MinOrganics { get; set; }
            public short MaxOrganics { get; set; }
            public short MinGeologicals { get; set; }
            public short MaxGeologicals { get; set; }
            public short MinRings { get; set; }
            public short MaxRings { get; set; }
            public short MinLandables { get; set; }
            public short MaxLandables { get; set; }
            public short MinWalkables { get; set; }
            public short MaxWalkables { get; set; }
            public short MinTerraformables { get; set; }
            public short MaxTerraformables { get; set; }
            public short MinVolcanics { get; set; }
            public short MaxVolcanics { get; set; }
            public int MaxDistanceToRegionCentre { get; set; }
            public string? HotspotTypes { get; set; }
        }
    }
}
