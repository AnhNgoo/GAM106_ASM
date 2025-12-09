using GAM106ASM.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GAM106ASM.Pages.Admin
{
    public class DashboardModel : PageModel
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public DashboardModel(AppDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        public string AdminEmail { get; set; } = "";
        public string? AdminAvatarUrl { get; set; }
        public int TotalPlayers { get; set; }
        public int TotalTransactions { get; set; }
        public int TotalQuests { get; set; }
        public int TotalItems { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            // Check if admin is logged in
            var adminPlayerId = HttpContext.Session.GetInt32("AdminPlayerId");
            var adminEmail = HttpContext.Session.GetString("AdminEmail");

            if (!adminPlayerId.HasValue || string.IsNullOrEmpty(adminEmail))
            {
                return RedirectToPage("/Index");
            }

            AdminEmail = adminEmail;

            // Load admin avatar
            var admin = await _context.Players.FindAsync(adminPlayerId.Value);
            if (admin != null)
            {
                AdminAvatarUrl = admin.AvatarUrl;
            }

            // Load statistics
            TotalPlayers = await _context.Players.CountAsync();
            TotalTransactions = await _context.Transactions.CountAsync();
            TotalQuests = await _context.Quests.CountAsync();
            TotalItems = await _context.ItemSalesSheets.CountAsync();

            return Page();
        }

        public IActionResult OnPostLogout()
        {
            HttpContext.Session.Clear();
            return RedirectToPage("/Index");
        }

        public async Task<IActionResult> OnPostUpdateProfileAsync(IFormFile? avatarFile, string? newPassword, string? confirmPassword)
        {
            var adminPlayerId = HttpContext.Session.GetInt32("AdminPlayerId");
            if (!adminPlayerId.HasValue)
            {
                return RedirectToPage("/Index");
            }

            var admin = await _context.Players.FindAsync(adminPlayerId.Value);
            if (admin == null)
            {
                return RedirectToPage("/Index");
            }

            // Handle avatar upload
            if (avatarFile != null && avatarFile.Length > 0)
            {
                // Delete old avatar if exists
                if (!string.IsNullOrEmpty(admin.AvatarUrl) && admin.AvatarUrl.StartsWith("/uploads/"))
                {
                    var oldFilePath = Path.Combine(_environment.WebRootPath, admin.AvatarUrl.TrimStart('/'));
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

                admin.AvatarUrl = $"/uploads/avatars/{uniqueFileName}";
            }

            // Handle password change
            if (!string.IsNullOrEmpty(newPassword))
            {
                if (newPassword != confirmPassword)
                {
                    TempData["ProfileMessage"] = "Passwords do not match!";
                    return RedirectToPage();
                }

                admin.LoginPassword = newPassword;
            }

            await _context.SaveChangesAsync();
            TempData["ProfileMessage"] = "Profile updated successfully!";

            return RedirectToPage();
        }
    }
}
