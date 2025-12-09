using GAM106ASM.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GAM106ASM.Pages.Admin
{
    public class ItemsModel : PageModel
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public ItemsModel(AppDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        public List<ItemSalesSheet> Items { get; set; } = new();
        public List<ItemType> ItemTypes { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            if (!IsAdminLoggedIn())
                return RedirectToPage("/Index");

            Items = await _context.ItemSalesSheets
                .Include(i => i.ItemType)
                .OrderBy(i => i.ItemSheetId)
                .ToListAsync();

            ItemTypes = await _context.ItemTypes.ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostAddAsync(string name, int typeId, int price, string? imageUrl, IFormFile? imageFile)
        {
            if (!IsAdminLoggedIn())
                return RedirectToPage("/Index");

            string? finalImageUrl = imageUrl;

            if (imageFile != null && imageFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "items");
                Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = $"{Guid.NewGuid()}_{imageFile.FileName}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(fileStream);
                }

                finalImageUrl = $"/uploads/items/{uniqueFileName}";
            }

            var newItem = new ItemSalesSheet
            {
                ItemVersionName = name,
                ItemTypeId = typeId,
                PurchaseValue = price,
                ImageUrl = finalImageUrl
            };

            _context.ItemSalesSheets.Add(newItem);
            await _context.SaveChangesAsync();

            TempData["Message"] = $"Item '{name}' added successfully!";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostEditAsync(int id, string name, int typeId, int price, string? imageUrl, IFormFile? imageFile)
        {
            if (!IsAdminLoggedIn())
                return RedirectToPage("/Index");

            var item = await _context.ItemSalesSheets.FindAsync(id);
            if (item != null)
            {
                string? finalImageUrl = imageUrl;

                if (imageFile != null && imageFile.Length > 0)
                {
                    // Delete old image if exists
                    if (!string.IsNullOrEmpty(item.ImageUrl) && item.ImageUrl.StartsWith("/uploads/"))
                    {
                        var oldFilePath = Path.Combine(_environment.WebRootPath, item.ImageUrl.TrimStart('/'));
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                    }

                    var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "items");
                    Directory.CreateDirectory(uploadsFolder);

                    var uniqueFileName = $"{Guid.NewGuid()}_{imageFile.FileName}";
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(fileStream);
                    }

                    finalImageUrl = $"/uploads/items/{uniqueFileName}";
                }

                item.ItemVersionName = name;
                item.ItemTypeId = typeId;
                item.PurchaseValue = price;
                item.ImageUrl = finalImageUrl;

                await _context.SaveChangesAsync();
                TempData["Message"] = $"Item '{name}' updated successfully!";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            if (!IsAdminLoggedIn())
                return RedirectToPage("/Index");

            var item = await _context.ItemSalesSheets.FindAsync(id);
            if (item != null)
            {
                _context.ItemSalesSheets.Remove(item);
                await _context.SaveChangesAsync();
                TempData["Message"] = $"Item deleted successfully!";
            }

            return RedirectToPage();
        }

        private bool IsAdminLoggedIn()
        {
            return HttpContext.Session.GetInt32("AdminPlayerId").HasValue;
        }
    }
}
