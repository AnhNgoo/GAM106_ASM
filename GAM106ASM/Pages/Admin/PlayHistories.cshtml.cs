using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using GAM106ASM.Models;

namespace GAM106ASM.Pages.Admin
{
    public class PlayHistoriesModel : PageModel
    {
        private readonly AppDbContext _context;

        public PlayHistoriesModel(AppDbContext context)
        {
            _context = context;
        }

        public List<PlayHistory> PlayHistories { get; set; } = new();
        public List<Player> Players { get; set; } = new();
        public List<GameMode> GameModes { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToPage("/Admin/Login");
            }

            PlayHistories = await _context.PlayHistories
                .Include(ph => ph.Player)
                .Include(ph => ph.Mode)
                .OrderByDescending(ph => ph.StartTime)
                .ToListAsync();

            Players = await _context.Players.OrderBy(p => p.EmailAccount).ToListAsync();
            GameModes = await _context.GameModes.OrderBy(gm => gm.ModeName).ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostAddAsync(int playerId, int modeId, DateTime startTime, DateTime? endTime)
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToPage("/Admin/Login");
            }

            var playHistory = new PlayHistory
            {
                PlayerId = playerId,
                ModeId = modeId,
                StartTime = startTime,
                EndTime = endTime
            };

            _context.PlayHistories.Add(playHistory);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Play session recorded successfully!";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int playerId, int modeId, DateTime startTime)
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToPage("/Admin/Login");
            }

            var playHistory = await _context.PlayHistories
                .FirstOrDefaultAsync(ph => ph.PlayerId == playerId && ph.ModeId == modeId && ph.StartTime == startTime);

            if (playHistory != null)
            {
                _context.PlayHistories.Remove(playHistory);
                await _context.SaveChangesAsync();
                TempData["Message"] = "Play session deleted successfully!";
            }

            return RedirectToPage();
        }

        private bool IsAdminLoggedIn()
        {
            return HttpContext.Session.GetInt32("AdminPlayerId").HasValue;
        }
    }
}
