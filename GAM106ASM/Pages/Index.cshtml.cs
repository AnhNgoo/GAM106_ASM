using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using GAM106ASM.Models;
using Microsoft.EntityFrameworkCore;

namespace GAM106ASM.Pages
{
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _context;

        public IndexModel(AppDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public string? Email { get; set; }

        [BindProperty]
        public string? Password { get; set; }

        public string? ErrorMessage { get; set; }

        public IActionResult OnGet()
        {
            // If already logged in, redirect
            var role = HttpContext.Session.GetString("Role");
            if (!string.IsNullOrEmpty(role))
            {
                if (role == "Admin") return RedirectToPage("/Admin/Dashboard");
                if (role == "Player") return RedirectToPage("/Member/Dashboard");
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Password))
            {
                ErrorMessage = "Vui lòng nhập email và mật khẩu.";
                return Page();
            }

            // 1. Check Player in Database
            var player = await _context.Players
                .FirstOrDefaultAsync(p => p.EmailAccount == Email && p.LoginPassword == Password);

            if (player != null)
            {
                // Check if player is Admin
                if (player.Role == "admin")
                {
                    HttpContext.Session.SetString("Role", "Admin");
                    HttpContext.Session.SetInt32("AdminPlayerId", player.PlayerId);
                    HttpContext.Session.SetString("AdminEmail", player.EmailAccount);
                    return RedirectToPage("/Admin/Dashboard");
                }

                // Regular Player
                HttpContext.Session.SetString("Role", "Player");
                HttpContext.Session.SetInt32("MemberPlayerId", player.PlayerId);
                HttpContext.Session.SetString("Username", player.EmailAccount);
                return RedirectToPage("/Member/Dashboard");
            }

            ErrorMessage = "Email hoặc mật khẩu không đúng.";
            return Page();
        }
    }
}
