using GAM106ASM.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GAM106ASM.Pages.Admin
{
    public class CharactersModel : PageModel
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public CharactersModel(AppDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        public List<Character> Characters { get; set; } = new();
        public List<Player> Players { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            if (!IsAdminLoggedIn())
                return RedirectToPage("/Index");

            Characters = await _context.Characters
                .Include(c => c.Player)
                .OrderBy(c => c.CharacterId)
                .ToListAsync();

            Players = await _context.Players.ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostAddAsync(int playerId, string characterName, string? sex, IFormFile? skinFile, string? skin)
        {
            if (!IsAdminLoggedIn())
                return RedirectToPage("/Index");

            string? finalSkin = skin;

            // Handle file upload
            if (skinFile != null && skinFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "skins");
                Directory.CreateDirectory(uploadsFolder);
                var uniqueFileName = $"{Guid.NewGuid()}_{skinFile.FileName}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await skinFile.CopyToAsync(fileStream);
                }
                finalSkin = $"/uploads/skins/{uniqueFileName}";
            }

            var newCharacter = new Character
            {
                PlayerId = playerId,
                CharacterName = characterName,
                Sex = sex,
                Skin = finalSkin
            };

            _context.Characters.Add(newCharacter);
            await _context.SaveChangesAsync();

            TempData["Message"] = $"Character '{characterName}' added successfully!";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostEditAsync(int id, string characterName, string? sex, IFormFile? skinFile, string? skin)
        {
            if (!IsAdminLoggedIn())
                return RedirectToPage("/Index");

            var character = await _context.Characters.FindAsync(id);
            if (character != null)
            {
                // Handle file upload
                if (skinFile != null && skinFile.Length > 0)
                {
                    // Delete old file if exists
                    if (!string.IsNullOrEmpty(character.Skin) && character.Skin.StartsWith("/uploads/"))
                    {
                        var oldFilePath = Path.Combine(_environment.WebRootPath, character.Skin.TrimStart('/'));
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                    }

                    var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "skins");
                    Directory.CreateDirectory(uploadsFolder);
                    var uniqueFileName = $"{Guid.NewGuid()}_{skinFile.FileName}";
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await skinFile.CopyToAsync(fileStream);
                    }
                    character.Skin = $"/uploads/skins/{uniqueFileName}";
                }
                else if (!string.IsNullOrEmpty(skin))
                {
                    character.Skin = skin;
                }

                character.CharacterName = characterName;
                character.Sex = sex;

                await _context.SaveChangesAsync();
                TempData["Message"] = $"Character '{characterName}' updated successfully!";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            if (!IsAdminLoggedIn())
                return RedirectToPage("/Index");

            var character = await _context.Characters.FindAsync(id);
            if (character != null)
            {
                _context.Characters.Remove(character);
                await _context.SaveChangesAsync();
                TempData["Message"] = "Character deleted successfully!";
            }

            return RedirectToPage();
        }

        private bool IsAdminLoggedIn()
        {
            return HttpContext.Session.GetInt32("AdminPlayerId").HasValue;
        }
    }
}
