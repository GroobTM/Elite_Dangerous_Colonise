using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace elite_dangerous_colonise.Pages
{
    public class ErrorModel : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public int? StatusCode { get; set; } = 500;

        public void OnGet(int? statusCode = null)
        {
            StatusCode = statusCode ?? HttpContext.Response.StatusCode;

            if (StatusCode == 200)
            {
                StatusCode = 500;
                HttpContext.Response.StatusCode = 500;
            }
        }
    }
}
