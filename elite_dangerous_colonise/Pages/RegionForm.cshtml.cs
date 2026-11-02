using elite_dangerous_colonise.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.RateLimiting;
using System.ComponentModel.DataAnnotations;

namespace elite_dangerous_colonise.Pages
{
    [EnableRateLimiting("RegionFormLimit")]
    public class RegionFormModel : PageModel
    {
        private readonly AppLogger logger;

        [TempData]
        public bool Post { get; set; } = false;
        [TempData]
        public DateTime LoadTime { get; set; }

        public RegionFormModel(AppLogger logger)
        {
            this.logger = logger;
        }

        [BindProperty]
        public RegionForm FormInput { get; set; } = new RegionForm();

        public IActionResult OnGet()
        {
            if (HttpContext.Session.GetString("PassedCaptcha") != "true")
            {
                string returnUrl = $"{Request.Path}{Request.QueryString}";

                return RedirectToPage("/Captcha", new { ReturnUrl = returnUrl });
            }

            LoadTime = DateTime.UtcNow;

            return Page();
        }

        public IActionResult OnPost()
        {
            if(!string.IsNullOrEmpty(FormInput.EmailAddress) || ((DateTime.UtcNow - LoadTime).TotalSeconds < 5))
            {
                Post = true;
                return RedirectToPage();
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            // TODO process form

            Post = true;
            return RedirectToPage();
        }


        public class RegionForm()
        {
            [Required, Display(Name = "Region Name"), StringLength(75, MinimumLength = 2, ErrorMessage = "Region Name must be between 2 and 75 characters.")]
            public string Name { get; set; } = "";
            [Required, Display(Name = "Region Radius"), Range(50, 500, ErrorMessage = "Region Radius must be between 50 and 500.")]
            public int Radius { get; set; } = 50;
            [Required, Display(Name = "Region Centre System"), StringLength(75, MinimumLength = 2, ErrorMessage = "Region Centre System must be between 2 and 75 characters.")]
            public string CentreSystem { get; set; } = "";
            [Display(Name = "Region Community Hub (Optional)"), StringLength(512, ErrorMessage = "Region Community Hub must be less than or equal to 512 characters.")]
            public string? Community { get; set; }
            [Display(Name = "Additional Notes (Optional)"), StringLength(2048, ErrorMessage = "Additional Notes must be less than or equal to 2048 characters.")]
            public string? Notes { get; set; }
            // Honeypot field
            public string? EmailAddress { get; set; }
        }
    }
}