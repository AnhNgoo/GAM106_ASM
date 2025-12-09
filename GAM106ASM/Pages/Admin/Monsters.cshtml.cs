using GAM106ASM.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GAM106ASM.Pages.Admin
{
    public class MonstersModel : PageModel
    {
        private readonly AppDbContext _context;

        public MonstersModel(AppDbContext context)
        {
            _context = context;
        }

        public List<Monster> Monsters { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            if (!IsAdminLoggedIn())
                return RedirectToPage("/Admin/Login");

            Monsters = await _context.Monsters.OrderBy(m => m.MonsterId).ToListAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAddAsync(string name, int xpReward)
        {
            if (!IsAdminLoggedIn())
                return RedirectToPage("/Admin/Login");

            var newMonster = new Monster
            {
                MonsterName = name,
                ExperienceReward = xpReward
            };

            _context.Monsters.Add(newMonster);
            await _context.SaveChangesAsync();

            TempData["Message"] = $"Monster '{name}' added successfully!";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostEditAsync(int id, string name, int xpReward)
        {
            if (!IsAdminLoggedIn())
                return RedirectToPage("/Admin/Login");

            var monster = await _context.Monsters.FindAsync(id);
            if (monster != null)
            {
                monster.MonsterName = name;
                monster.ExperienceReward = xpReward;

                await _context.SaveChangesAsync();
                TempData["Message"] = $"Monster '{name}' updated successfully!";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            if (!IsAdminLoggedIn())
                return RedirectToPage("/Admin/Login");

            var monster = await _context.Monsters.FindAsync(id);
            if (monster != null)
            {
                _context.Monsters.Remove(monster);
                await _context.SaveChangesAsync();
                TempData["Message"] = "Monster deleted successfully!";
            }

            return RedirectToPage();
        }

        private bool IsAdminLoggedIn()
        {
            return HttpContext.Session.GetInt32("AdminPlayerId").HasValue;
        }
    }
}
