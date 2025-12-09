using GAM106ASM.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GAM106ASM.Pages.Admin
{
    public class ResourcesModel : PageModel
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public ResourcesModel(AppDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        public List<Resource> Resources { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            if (!IsAdminLoggedIn())
                return RedirectToPage("/Admin/Login");

            Resources = await _context.Resources.OrderBy(r => r.ResourceId).ToListAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAddAsync(string name, string? description, IFormFile? textureFile, string? textureUrl)
        {
            if (!IsAdminLoggedIn())
                return RedirectToPage("/Index");

            string? finalTextureUrl = textureUrl;

            // Handle file upload
            if (textureFile != null && textureFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "resources");
                Directory.CreateDirectory(uploadsFolder);
                var uniqueFileName = $"{Guid.NewGuid()}_{textureFile.FileName}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await textureFile.CopyToAsync(fileStream);
                }
                finalTextureUrl = $"/uploads/resources/{uniqueFileName}";
            }

            var newResource = new Resource
            {
                ResourceName = name,
                Description = description,
                TextureUrl = finalTextureUrl
            };

            _context.Resources.Add(newResource);
            await _context.SaveChangesAsync();

            TempData["Message"] = $"Resource '{name}' added successfully!";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostEditAsync(int id, string name, string? description, IFormFile? textureFile, string? textureUrl)
        {
            if (!IsAdminLoggedIn())
                return RedirectToPage("/Index");

            var resource = await _context.Resources.FindAsync(id);
            if (resource != null)
            {
                // Handle file upload
                if (textureFile != null && textureFile.Length > 0)
                {
                    // Delete old file if exists
                    if (!string.IsNullOrEmpty(resource.TextureUrl) && resource.TextureUrl.StartsWith("/uploads/"))
                    {
                        var oldFilePath = Path.Combine(_environment.WebRootPath, resource.TextureUrl.TrimStart('/'));
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                    }

                    var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "resources");
                    Directory.CreateDirectory(uploadsFolder);
                    var uniqueFileName = $"{Guid.NewGuid()}_{textureFile.FileName}";
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await textureFile.CopyToAsync(fileStream);
                    }
                    resource.TextureUrl = $"/uploads/resources/{uniqueFileName}";
                }
                else if (!string.IsNullOrEmpty(textureUrl))
                {
                    resource.TextureUrl = textureUrl;
                }

                resource.ResourceName = name;
                resource.Description = description;

                await _context.SaveChangesAsync();
                TempData["Message"] = $"Resource '{name}' updated successfully!";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            if (!IsAdminLoggedIn())
                return RedirectToPage("/Index");

            var resource = await _context.Resources.FindAsync(id);
            if (resource != null)
            {
                _context.Resources.Remove(resource);
                await _context.SaveChangesAsync();
                TempData["Message"] = "Resource deleted successfully!";
            }

            return RedirectToPage();
        }

        private bool IsAdminLoggedIn()
        {
            return HttpContext.Session.GetInt32("AdminPlayerId").HasValue;
        }
    }
}
