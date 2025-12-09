using GAM106ASM.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GAM106ASM.Pages.Admin
{
    public class TestUnityAPIModel : PageModel
    {
        public IActionResult OnGet()
        {
            // Check if admin is logged in
            var adminPlayerId = HttpContext.Session.GetInt32("AdminPlayerId");
            var adminEmail = HttpContext.Session.GetString("AdminEmail");

            if (!adminPlayerId.HasValue || string.IsNullOrEmpty(adminEmail))
            {
                return RedirectToPage("/Index");
            }

            return Page();
        }
    }
}
