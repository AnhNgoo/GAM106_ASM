using GAM106ASM.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GAM106ASM.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CharacterController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CharacterController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Character/{playerId}
        [HttpGet("{playerId}")]
        public async Task<IActionResult> GetCharacterByPlayerId(int playerId)
        {
            var character = await _context.Characters
                .Include(c => c.Player)
                .FirstOrDefaultAsync(c => c.PlayerId == playerId);

            if (character == null)
            {
                return NotFound(new { message = "Character not found for this player" });
            }

            return Ok(new
            {
                characterId = character.CharacterId,
                playerId = character.PlayerId,
                characterName = character.CharacterName,
                sex = character.Sex,
                skin = character.Skin,
                // Player stats for GameHub
                avatarUrl = character.Player.AvatarUrl,
                experiencePoints = character.Player.ExperiencePoints,
                healthBar = character.Player.HealthBar,
                foodBar = character.Player.FoodBar,
                emailAccount = character.Player.EmailAccount
            });
        }

        // GET: api/Character/check/{playerId} - Kiểm tra xem player có character chưa
        [HttpGet("check/{playerId}")]
        public async Task<IActionResult> CheckCharacterExists(int playerId)
        {
            var character = await _context.Characters
                .Include(c => c.Player)
                .FirstOrDefaultAsync(c => c.PlayerId == playerId);

            if (character == null)
            {
                return Ok(new { exists = false, message = "No character found. Please create one." });
            }

            return Ok(new
            {
                exists = true,
                character = new
                {
                    character.CharacterId,
                    character.PlayerId,
                    character.CharacterName,
                    character.Sex,
                    character.Skin
                }
            });
        }

        // POST: api/Character/create
        [HttpPost("create")]
        public async Task<IActionResult> CreateCharacter([FromBody] CreateCharacterDto dto)
        {
            // Check if player exists
            var player = await _context.Players.FindAsync(dto.PlayerId);
            if (player == null)
            {
                return NotFound(new { message = "Player not found" });
            }

            // Check if player already has a character
            var existingCharacter = await _context.Characters
                .FirstOrDefaultAsync(c => c.PlayerId == dto.PlayerId);
            if (existingCharacter != null)
            {
                return BadRequest(new { message = "Player already has a character" });
            }

            // Auto-assign skin based on sex (nam = steve, nu = alex)
            string skinUrl = dto.Sex.ToLower() == "nam" ? "/Image/steve.png" : "/Image/alex.png";

            var character = new Character
            {
                PlayerId = dto.PlayerId,
                CharacterName = dto.CharacterName,
                Sex = dto.Sex,
                Skin = skinUrl
            };

            _context.Characters.Add(character);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Character created successfully",
                character = new
                {
                    characterId = character.CharacterId,
                    playerId = character.PlayerId,
                    characterName = character.CharacterName,
                    sex = character.Sex,
                    skin = character.Skin
                }
            });
        }

        // PUT: api/Character/{characterId}/skin
        [HttpPut("{characterId}/skin")]
        public async Task<IActionResult> UpdateCharacterSkin(int characterId, [FromBody] UpdateSkinDto dto)
        {
            var character = await _context.Characters.FindAsync(characterId);
            if (character == null)
            {
                return NotFound(new { message = "Character not found" });
            }

            character.Skin = dto.SkinUrl;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Skin updated successfully", character });
        }
    }

    // DTOs
    public class CreateCharacterDto
    {
        public int PlayerId { get; set; }
        public string CharacterName { get; set; } = null!;
        public string Sex { get; set; } = null!; // "nam" or "nu"
    }

    public class UpdateSkinDto
    {
        public string SkinUrl { get; set; } = null!;
    }
}
