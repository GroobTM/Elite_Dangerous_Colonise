using elite_dangerous_colonise.Models.Internal;
using elite_dangerous_colonise.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace elite_dangerous_colonise.Pages
{
    public class RegionsModel : PageModel
    {
        private readonly RegionStore regionStore;

        public IReadOnlyList<Region> Regions { get; private set; }

        public RegionsModel (RegionStore regionStore)
        {
            this.regionStore = regionStore;
        }

        public IActionResult OnGet()
        {
            if (HttpContext.Session.GetString("PassedCaptcha") != "true")
            {
                string returnUrl = $"{Request.Path}{Request.QueryString}";

                return RedirectToPage("/Captcha", new { ReturnUrl = returnUrl });
            }

            Regions = regionStore.GetActiveRegions();

            return Page();
        }
    }
}
