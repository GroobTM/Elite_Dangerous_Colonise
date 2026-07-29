using elite_dangerous_colonise.Models.Database_Results;
using elite_dangerous_colonise.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Npgsql;
using System.Text.RegularExpressions;


namespace elite_dangerous_colonise.Pages;

public class IndexModel : PageModel
{
    private const int MAX_QUERY_ATTEMPTS = 3;
    private const int QUERY_ATTEMPT_DELAY = 1;

    private readonly NpgsqlDataSource dataSource;
    private readonly AppLogger logger;

    public SelectMaxSearchValuesResult MaxValues { get; private set; }
    public List<string> HotspotTypes { get; private set; }

    [BindProperty]
    public string ColonisedSystem { get; set; }

    [BindProperty]
    public string Faction { get; set; }

    [BindProperty]
    public string SortOrder { get; set; }


    public IndexModel(NpgsqlDataSource dataSource, AppLogger logger)
    {
        this.dataSource = dataSource;
        this.logger = logger;
    }

    public async Task<IActionResult> OnGet()
    {
        if (HttpContext.Session.GetString("PassedCaptcha") != "true")
        {
            string returnUrl = $"{Request.Path}{Request.QueryString}";

            return RedirectToPage("/Captcha", new { ReturnUrl = returnUrl });
        }


        try
        {
            MaxValues = await SelectMaxSearchValues() ?? throw new Exception("MaxValues is null");
            HotspotTypes = await SelectHotspotTypes() ?? throw new Exception("HotspotTypes is null");

            return Page();
        }
        catch (NpgsqlException ex)
        {
            logger.LogError("Index Page", 0, "A database error occurred.", ex);

            return RedirectToPage("/Error", new { statusCode = 500 });
        }

        catch (Exception ex)
        {
            logger.LogError("Index Page", 1, ex);

            return RedirectToPage("/Error", new { statusCode = 500 });
        }
    }

    public void OnPost()
    {

    }

    private async Task<SelectMaxSearchValuesResult?> SelectMaxSearchValues()
    {
        for (int attempt = 1; attempt <= MAX_QUERY_ATTEMPTS; attempt++)
        {
            try
            {
                await using (NpgsqlConnection conn = await dataSource.OpenConnectionAsync())
                {
                    await using (NpgsqlCommand command = new NpgsqlCommand("SELECT * FROM \"MaxSearchValues\"", conn))
                    {
                        await using (NpgsqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            await reader.ReadAsync();

                            return new SelectMaxSearchValuesResult(
                                reader.GetInt32(0),
                                reader.GetInt32(1),
                                reader.GetInt32(2),
                                reader.GetInt32(3),
                                reader.GetInt32(4),
                                reader.GetInt32(5),
                                reader.GetInt32(6),
                                reader.GetInt32(7),
                                reader.GetInt32(8),
                                reader.GetInt32(9),
                                reader.GetInt32(10),
                                reader.GetInt32(11),
                                reader.GetInt32(12),
                                reader.GetInt32(13),
                                reader.GetInt32(14),
                                reader.GetInt32(15),
                                reader.GetInt32(16),
                                reader.GetInt32(17),
                                reader.GetInt32(18),
                                reader.GetInt32(19)
                                );
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

        return null;
    }

    private async Task<List<string>?> SelectHotspotTypes()
    {
        for (int attempt = 1; attempt <= MAX_QUERY_ATTEMPTS; attempt++)
        {
            List<string> result = new List<string>();

            try 
            {
                await using (NpgsqlConnection conn = await dataSource.OpenConnectionAsync())
                {
                    await using (NpgsqlCommand command = new NpgsqlCommand("SELECT DISTINCT \"hotspotType\" FROM \"Hotspots\";", conn))
                    {
                        await using (NpgsqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                result.Add(Regex.Replace(reader.GetString(0), "(\\B[A-Z])", " $1"));
                            }
                        }
                    }
                }

                return result;
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

        return null;
    }
}
