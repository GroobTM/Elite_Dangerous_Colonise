using elite_dangerous_colonise.Models.Internal;
using elite_dangerous_colonise.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace elite_dangerous_colonise.Pages
{
    public class RegionsModel : PageModel
    {
        private readonly AppLogger logger;
        private readonly RegionStore regionStore;

        public List<Region> Regions { get; private set; }

        public RegionsModel (AppLogger logger, RegionStore regionStore)
        {
            this.logger = logger;
            this.regionStore = regionStore;
        }

        public IActionResult OnGet()
        {
            if (HttpContext.Session.GetString("PassedCaptcha") != "true")
            {
                string returnUrl = $"{Request.Path}{Request.QueryString}";

                return RedirectToPage("/Captcha", new { ReturnUrl = returnUrl });
            }

            Regions = new List<Region>(regionStore.GetRegions());
            //Regions.RemoveAll(region => region.Name == "Colonia" || region.Name == "Sagittarius A*");

            return Page();
        }
    }
}
