using GAM106ASM.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GAM106ASM.Pages.Admin
{
    public class GameModesModel : PageModel
    {
        private readonly AppDbContext _context;

        public GameModesModel(AppDbContext context)
        {
            _context = context;
        }

        public List<GameMode> GameModes { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            if (!IsAdminLoggedIn())
                return RedirectToPage("/Admin/Login");

            GameModes = await _context.GameModes.OrderBy(g => g.ModeId).ToListAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAddAsync(string modeName)
        {
            if (!IsAdminLoggedIn())
                return RedirectToPage("/Admin/Login");

            var newMode = new GameMode { ModeName = modeName };
            _context.GameModes.Add(newMode);
            await _context.SaveChangesAsync();

            TempData["Message"] = $"Game Mode '{modeName}' added successfully!";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostEditAsync(int id, string modeName)
        {
            if (!IsAdminLoggedIn())
                return RedirectToPage("/Admin/Login");

            var mode = await _context.GameModes.FindAsync(id);
            if (mode != null)
            {
                mode.ModeName = modeName;
                await _context.SaveChangesAsync();
                TempData["Message"] = $"Game Mode '{modeName}' updated successfully!";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            if (!IsAdminLoggedIn())
                return RedirectToPage("/Admin/Login");

            var mode = await _context.GameModes.FindAsync(id);
            if (mode != null)
            {
                _context.GameModes.Remove(mode);
                await _context.SaveChangesAsync();
                TempData["Message"] = "Game Mode deleted successfully!";
            }

            return RedirectToPage();
        }

        private bool IsAdminLoggedIn()
        {
            return HttpContext.Session.GetInt32("AdminPlayerId").HasValue;
        }
    }
}
