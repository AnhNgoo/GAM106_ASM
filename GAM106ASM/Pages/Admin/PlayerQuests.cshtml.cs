using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using GAM106ASM.Models;

namespace GAM106ASM.Pages.Admin
{
    public class PlayerQuestsModel : PageModel
    {
        private readonly AppDbContext _context;

        public PlayerQuestsModel(AppDbContext context)
        {
            _context = context;
        }

        public List<PlayerQuest> PlayerQuests { get; set; } = new();
        public List<Player> Players { get; set; } = new();
        public List<Quest> Quests { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToPage("/Admin/Login");
            }

            PlayerQuests = await _context.PlayerQuests
                .Include(pq => pq.Player)
                .Include(pq => pq.Quest)
                .OrderByDescending(pq => pq.CompletionTime)
                .ToListAsync();

            Players = await _context.Players.OrderBy(p => p.EmailAccount).ToListAsync();
            Quests = await _context.Quests.OrderBy(q => q.QuestName).ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostAddAsync(int playerId, int questId, string status, DateTime? completionTime)
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToPage("/Admin/Login");
            }

            var playerQuest = new PlayerQuest
            {
                PlayerId = playerId,
                QuestId = questId,
                Status = status,
                CompletionTime = completionTime
            };

            _context.PlayerQuests.Add(playerQuest);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Player quest added successfully!";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostEditAsync(int playerId, int questId, string status, DateTime? completionTime)
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToPage("/Admin/Login");
            }

            var playerQuest = await _context.PlayerQuests
                .FirstOrDefaultAsync(pq => pq.PlayerId == playerId && pq.QuestId == questId);

            if (playerQuest != null)
            {
                playerQuest.Status = status;
                playerQuest.CompletionTime = completionTime;
                await _context.SaveChangesAsync();
                TempData["Message"] = "Player quest updated successfully!";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int playerId, int questId)
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToPage("/Admin/Login");
            }

            var playerQuest = await _context.PlayerQuests
                .FirstOrDefaultAsync(pq => pq.PlayerId == playerId && pq.QuestId == questId);

            if (playerQuest != null)
            {
                _context.PlayerQuests.Remove(playerQuest);
                await _context.SaveChangesAsync();
                TempData["Message"] = "Player quest deleted successfully!";
            }

            return RedirectToPage();
        }

        private bool IsAdminLoggedIn()
        {
            return HttpContext.Session.GetInt32("AdminPlayerId").HasValue;
        }
    }
}
