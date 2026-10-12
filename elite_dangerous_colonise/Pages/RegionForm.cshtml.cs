using elite_dangerous_colonise.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace elite_dangerous_colonise.Pages
{
    public class RegionFormModel : PageModel
    {
        private readonly AppLogger logger;

        public bool Post { get; private set; } = false;

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

            return Page();
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            Post = true;

            return Page();
        }


        public class RegionForm()
        {
            [Required, Display(Name = "Region Name"), StringLength(75, MinimumLength = 2, ErrorMessage = "Region Name must be between 2 and 75 characters.")]
            public string Name { get; set; } = "";
            [Required, Display(Name = "Region Radius"), Range(50, 500, ErrorMessage = "Region Radius must be between 50 and 500.")]
            public int Radius { get; set; } = 50;
            [Required, Display(Name = "Region Centre System"), StringLength(75, MinimumLength = 2, ErrorMessage = "Region Centre System must be between 2 and 75 characters.")]
            public string CentreSystem { get; set; } = "";
            [Display(Name = "Region Community Hub"), StringLength(512, ErrorMessage = "Region Community Hub must be less than or equal to 512 characters.")]
            public string? Community { get; set; }
        }
    }
}