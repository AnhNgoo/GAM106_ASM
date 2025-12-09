using GAM106ASM.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GAM106ASM.Pages.Admin
{
    public class PlayersModel : PageModel
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public PlayersModel(AppDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        public List<Player> Players { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            if (!IsAdminLoggedIn())
                return RedirectToPage("/Index");

            Players = await _context.Players.ToListAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAddAsync(string email, string password, int experiencePoints, int healthBar, int foodBar, string role, string? avatarUrl, IFormFile? avatarFile)
        {
            if (!IsAdminLoggedIn())
                return RedirectToPage("/Index");

            string? finalAvatarUrl = avatarUrl;

            if (avatarFile != null && avatarFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "avatars");
                Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = $"{Guid.NewGuid()}_{avatarFile.FileName}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await avatarFile.CopyToAsync(fileStream);
                }

                finalAvatarUrl = $"/uploads/avatars/{uniqueFileName}";
            }

            var newPlayer = new Player
            {
                EmailAccount = email,
                LoginPassword = password,
                ExperiencePoints = experiencePoints,
                HealthBar = healthBar,
                FoodBar = foodBar,
                Role = role,
                AvatarUrl = finalAvatarUrl
            };

            _context.Players.Add(newPlayer);
            await _context.SaveChangesAsync();

            TempData["Message"] = $"Player '{email}' added successfully!";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostEditAsync(int id, string email, string? password, int experiencePoints, int healthBar, int foodBar, string role, string? avatarUrl, IFormFile? avatarFile)
        {
            if (!IsAdminLoggedIn())
                return RedirectToPage("/Index");

            var player = await _context.Players.FindAsync(id);
            if (player != null)
            {
                string? finalAvatarUrl = avatarUrl;

                if (avatarFile != null && avatarFile.Length > 0)
                {
                    // Delete old avatar if exists
                    if (!string.IsNullOrEmpty(player.AvatarUrl) && player.AvatarUrl.StartsWith("/uploads/"))
                    {
                        var oldFilePath = Path.Combine(_environment.WebRootPath, player.AvatarUrl.TrimStart('/'));
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                    }

                    var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "avatars");
                    Directory.CreateDirectory(uploadsFolder);

                    var uniqueFileName = $"{Guid.NewGuid()}_{avatarFile.FileName}";
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await avatarFile.CopyToAsync(fileStream);
                    }

                    finalAvatarUrl = $"/uploads/avatars/{uniqueFileName}";
                }

                player.EmailAccount = email;
                if (!string.IsNullOrEmpty(password))
                {
                    player.LoginPassword = password;
                }
                player.ExperiencePoints = experiencePoints;
                player.HealthBar = healthBar;
                player.FoodBar = foodBar;
                player.Role = role;
                player.AvatarUrl = finalAvatarUrl;

                await _context.SaveChangesAsync();
                TempData["Message"] = $"Player '{email}' updated successfully!";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            if (!IsAdminLoggedIn())
                return RedirectToPage("/Index");

            var player = await _context.Players.FindAsync(id);
            if (player != null)
            {
                _context.Players.Remove(player);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage();
        }

        private bool IsAdminLoggedIn()
        {
            return HttpContext.Session.GetInt32("AdminPlayerId").HasValue;
        }
    }
}
