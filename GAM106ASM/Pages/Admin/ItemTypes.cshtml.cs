using GAM106ASM.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GAM106ASM.Pages.Admin
{
    public class ItemTypesModel : PageModel
    {
        private readonly AppDbContext _context;

        public ItemTypesModel(AppDbContext context)
        {
            _context = context;
        }

        public List<ItemType> ItemTypes { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            if (!IsAdminLoggedIn())
                return RedirectToPage("/Admin/Login");

            ItemTypes = await _context.ItemTypes.OrderBy(t => t.ItemTypeId).ToListAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAddAsync(string itemTypeName)
        {
            if (!IsAdminLoggedIn())
                return RedirectToPage("/Admin/Login");

            var newType = new ItemType { ItemTypeName = itemTypeName };
            _context.ItemTypes.Add(newType);
            await _context.SaveChangesAsync();

            TempData["Message"] = $"Item Type '{itemTypeName}' added successfully!";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostEditAsync(int id, string itemTypeName)
        {
            if (!IsAdminLoggedIn())
                return RedirectToPage("/Admin/Login");

            var type = await _context.ItemTypes.FindAsync(id);
            if (type != null)
            {
                type.ItemTypeName = itemTypeName;
                await _context.SaveChangesAsync();
                TempData["Message"] = $"Item Type '{itemTypeName}' updated successfully!";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            if (!IsAdminLoggedIn())
                return RedirectToPage("/Admin/Login");

            var type = await _context.ItemTypes.FindAsync(id);
            if (type != null)
            {
                _context.ItemTypes.Remove(type);
                await _context.SaveChangesAsync();
                TempData["Message"] = "Item Type deleted successfully!";
            }

            return RedirectToPage();
        }

        private bool IsAdminLoggedIn()
        {
            return HttpContext.Session.GetInt32("AdminPlayerId").HasValue;
        }
    }
}
