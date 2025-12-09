using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using GAM106ASM.Models;

namespace GAM106ASM.Pages.Admin
{
    public class MonsterKillsModel : PageModel
    {
        private readonly AppDbContext _context;

        public MonsterKillsModel(AppDbContext context)
        {
            _context = context;
        }

        public List<MonsterKill> MonsterKills { get; set; } = new();
        public List<Player> Players { get; set; } = new();
        public List<Monster> Monsters { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToPage("/Admin/Login");
            }

            MonsterKills = await _context.MonsterKills
                .Include(mk => mk.Player)
                .Include(mk => mk.Monster)
                .OrderByDescending(mk => mk.KillTime)
                .ToListAsync();

            Players = await _context.Players.OrderBy(p => p.EmailAccount).ToListAsync();
            Monsters = await _context.Monsters.OrderBy(m => m.MonsterName).ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostAddAsync(int playerId, int monsterId, DateTime killTime, int quantity)
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToPage("/Admin/Login");
            }

            var monsterKill = new MonsterKill
            {
                PlayerId = playerId,
                MonsterId = monsterId,
                KillTime = killTime,
                Quantity = quantity
            };

            _context.MonsterKills.Add(monsterKill);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Monster kill recorded successfully!";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int playerId, int monsterId, DateTime killTime)
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToPage("/Admin/Login");
            }

            var monsterKill = await _context.MonsterKills
                .FirstOrDefaultAsync(mk => mk.PlayerId == playerId && mk.MonsterId == monsterId && mk.KillTime == killTime);

            if (monsterKill != null)
            {
                _context.MonsterKills.Remove(monsterKill);
                await _context.SaveChangesAsync();
                TempData["Message"] = "Monster kill deleted successfully!";
            }

            return RedirectToPage();
        }

        private bool IsAdminLoggedIn()
        {
            return HttpContext.Session.GetInt32("AdminPlayerId").HasValue;
        }
    }
}
