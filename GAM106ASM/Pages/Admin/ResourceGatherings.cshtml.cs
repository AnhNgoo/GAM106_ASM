using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using GAM106ASM.Models;

namespace GAM106ASM.Pages.Admin
{
    public class ResourceGatheringsModel : PageModel
    {
        private readonly AppDbContext _context;

        public ResourceGatheringsModel(AppDbContext context)
        {
            _context = context;
        }

        public List<ResourceGathering> ResourceGatherings { get; set; } = new();
        public List<Player> Players { get; set; } = new();
        public List<Resource> Resources { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToPage("/Admin/Login");
            }

            ResourceGatherings = await _context.ResourceGatherings
                .Include(rg => rg.Player)
                .Include(rg => rg.Resource)
                .OrderByDescending(rg => rg.GatheringTime)
                .ToListAsync();

            Players = await _context.Players.OrderBy(p => p.EmailAccount).ToListAsync();
            Resources = await _context.Resources.OrderBy(r => r.ResourceName).ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostAddAsync(int playerId, int resourceId, DateTime gatheringTime, int quantity)
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToPage("/Admin/Login");
            }

            var resourceGathering = new ResourceGathering
            {
                PlayerId = playerId,
                ResourceId = resourceId,
                GatheringTime = gatheringTime,
                Quantity = quantity
            };

            _context.ResourceGatherings.Add(resourceGathering);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Resource gathering recorded successfully!";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int playerId, int resourceId, DateTime gatheringTime)
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToPage("/Admin/Login");
            }

            var resourceGathering = await _context.ResourceGatherings
                .FirstOrDefaultAsync(rg => rg.PlayerId == playerId && rg.ResourceId == resourceId && rg.GatheringTime == gatheringTime);

            if (resourceGathering != null)
            {
                _context.ResourceGatherings.Remove(resourceGathering);
                await _context.SaveChangesAsync();
                TempData["Message"] = "Resource gathering deleted successfully!";
            }

            return RedirectToPage();
        }

        private bool IsAdminLoggedIn()
        {
            return HttpContext.Session.GetInt32("AdminPlayerId").HasValue;
        }
    }
}
