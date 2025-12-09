using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using GAM106ASM.Models;
using Microsoft.EntityFrameworkCore;

namespace GAM106ASM.Pages
{
    public class RegisterModel : PageModel
    {
        private readonly AppDbContext _context;

        public RegisterModel(AppDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public string? Email { get; set; }

        [BindProperty]
        public string? Password { get; set; }

        [BindProperty]
        public string? ConfirmPassword { get; set; }

        public void OnGet()
        {
            // Just display the registration form
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Validate inputs
            if (string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Password) || string.IsNullOrEmpty(ConfirmPassword))
            {
                TempData["RegisterError"] = "Vui lòng điền đầy đủ thông tin.";
                return Page();
            }

            if (Password != ConfirmPassword)
            {
                TempData["RegisterError"] = "Mật khẩu xác nhận không khớp.";
                return Page();
            }

            // Check if email already exists
            var existingPlayer = await _context.Players.FirstOrDefaultAsync(p => p.EmailAccount == Email);
            if (existingPlayer != null)
            {
                TempData["RegisterError"] = "Email đã được sử dụng.";
                return Page();
            }

            // Create new player
            var newPlayer = new Player
            {
                EmailAccount = Email,
                LoginPassword = Password,
                Role = "member",
                HealthBar = 20,
                FoodBar = 20,
                ExperiencePoints = 0,
                AvatarUrl = null
            };

            _context.Players.Add(newPlayer);
            await _context.SaveChangesAsync();

            // Auto-login after registration
            HttpContext.Session.SetString("Role", "Player");
            HttpContext.Session.SetInt32("MemberPlayerId", newPlayer.PlayerId);
            HttpContext.Session.SetString("Username", newPlayer.EmailAccount);

            TempData["RegisterMessage"] = "Đăng ký thành công! Chào mừng bạn đến với Minecraft Server.";
            return RedirectToPage("/Member/Dashboard");
        }
    }
}
