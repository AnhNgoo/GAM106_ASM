using GAM106ASM.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GAM106ASM.Pages.Admin
{
    public class QuestsModel : PageModel
    {
        private readonly AppDbContext _context;

        public QuestsModel(AppDbContext context)
        {
            _context = context;
        }

        public List<Quest> Quests { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            if (!IsAdminLoggedIn())
                return RedirectToPage("/Admin/Login");

            Quests = await _context.Quests
                .OrderBy(q => q.QuestId)
                .ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostAddAsync(string name, string? description, int xpReward)
        {
            if (!IsAdminLoggedIn())
                return RedirectToPage("/Admin/Login");

            var newQuest = new Quest
            {
                QuestName = name,
                Description = description,
                ExperienceReward = xpReward
            };

            _context.Quests.Add(newQuest);
            await _context.SaveChangesAsync();

            TempData["Message"] = $"Quest '{name}' added successfully!";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostEditAsync(int id, string name, string? description, int xpReward)
        {
            if (!IsAdminLoggedIn())
                return RedirectToPage("/Admin/Login");

            var quest = await _context.Quests.FindAsync(id);
            if (quest != null)
            {
                quest.QuestName = name;
                quest.Description = description;
                quest.ExperienceReward = xpReward;

                await _context.SaveChangesAsync();
                TempData["Message"] = $"Quest '{name}' updated successfully!";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            if (!IsAdminLoggedIn())
                return RedirectToPage("/Admin/Login");

            var quest = await _context.Quests.FindAsync(id);
            if (quest != null)
            {
                _context.Quests.Remove(quest);
                await _context.SaveChangesAsync();
                TempData["Message"] = "Quest deleted successfully!";
            }

            return RedirectToPage();
        }

        private bool IsAdminLoggedIn()
        {
            return HttpContext.Session.GetInt32("AdminPlayerId").HasValue;
        }
    }
}
