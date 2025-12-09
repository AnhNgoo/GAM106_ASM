using GAM106ASM.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GAM106ASM.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GameInfoController : ControllerBase
    {
        private readonly AppDbContext _context;

        public GameInfoController(AppDbContext context)
        {
            _context = context;
        }

        // Y3.1: GET: api/GameInfo/resources - Danh sách tất cả tài nguyên
        [HttpGet("resources")]
        public async Task<IActionResult> GetAllResources()
        {
            var resources = await _context.Resources
                .OrderBy(r => r.ResourceName)
                .Select(r => new
                {
                    r.ResourceId,
                    r.ResourceName,
                    r.Description,
                    r.TextureUrl
                })
                .ToListAsync();

            return Ok(resources);
        }

        // GET: api/GameInfo/resources/{resourceId}
        [HttpGet("resources/{resourceId}")]
        public async Task<IActionResult> GetResourceDetail(int resourceId)
        {
            var resource = await _context.Resources.FindAsync(resourceId);

            if (resource == null)
            {
                return NotFound(new { message = "Resource not found" });
            }

            // Get gathering statistics for this resource
            var gatheringStats = await _context.ResourceGatherings
                .Where(rg => rg.ResourceId == resourceId)
                .GroupBy(rg => rg.ResourceId)
                .Select(g => new
                {
                    TotalGathered = g.Sum(x => x.Quantity),
                    UniqueGatherers = g.Select(x => x.PlayerId).Distinct().Count()
                })
                .FirstOrDefaultAsync();

            return Ok(new
            {
                resource = new
                {
                    resource.ResourceId,
                    resource.ResourceName,
                    resource.Description,
                    resource.TextureUrl
                },
                stats = gatheringStats
            });
        }

        // Y3.2: GET: api/GameInfo/online-players - Danh sách người chơi đang online
        [HttpGet("online-players")]
        public async Task<IActionResult> GetOnlinePlayers()
        {
            // Lấy các session đang chơi (EndTime = null)
            var onlinePlayers = await _context.PlayHistories
                .Where(ph => ph.EndTime == null)
                .Include(ph => ph.Player)
                    .ThenInclude(p => p.Character)
                .Include(ph => ph.Mode)
                .Select(ph => new
                {
                    PlayerId = ph.Player.PlayerId,
                    Email = ph.Player.EmailAccount,
                    CharacterName = ph.Player.Character != null ? ph.Player.Character.CharacterName : "Steve",
                    AvatarUrl = ph.Player.AvatarUrl,
                    ExperiencePoints = ph.Player.ExperiencePoints,
                    HealthBar = ph.Player.HealthBar,
                    FoodBar = ph.Player.FoodBar,
                    CurrentMode = ph.Mode.ModeName,
                    SessionStartTime = ph.StartTime
                })
                .ToListAsync();

            return Ok(new
            {
                totalOnline = onlinePlayers.Count,
                players = onlinePlayers
            });
        }

        // GET: api/GameInfo/players-by-mode/{modeId}
        [HttpGet("players-by-mode/{modeId}")]
        public async Task<IActionResult> GetPlayersByGameMode(int modeId)
        {
            var mode = await _context.GameModes.FindAsync(modeId);
            if (mode == null)
            {
                return NotFound(new { message = "Game mode not found" });
            }

            var playersInMode = await _context.PlayHistories
                .Where(ph => ph.ModeId == modeId && ph.EndTime == null)
                .Include(ph => ph.Player)
                    .ThenInclude(p => p.Character)
                .Select(ph => new
                {
                    PlayerId = ph.Player.PlayerId,
                    Email = ph.Player.EmailAccount,
                    CharacterName = ph.Player.Character != null ? ph.Player.Character.CharacterName : "Steve",
                    AvatarUrl = ph.Player.AvatarUrl,
                    ExperiencePoints = ph.Player.ExperiencePoints,
                    SessionStartTime = ph.StartTime
                })
                .ToListAsync();

            return Ok(new
            {
                mode = mode.ModeName,
                playerCount = playersInMode.Count,
                players = playersInMode
            });
        }

        // GET: api/GameInfo/game-modes - Danh sách tất cả game modes
        [HttpGet("game-modes")]
        public async Task<IActionResult> GetAllGameModes()
        {
            var gameModes = await _context.GameModes
                .Select(gm => new
                {
                    gm.ModeId,
                    gm.ModeName,
                    ActivePlayers = _context.PlayHistories
                        .Count(ph => ph.ModeId == gm.ModeId && ph.EndTime == null)
                })
                .ToListAsync();

            return Ok(gameModes);
        }

        // GET: api/GameInfo/monsters - Danh sách tất cả quái vật
        [HttpGet("monsters")]
        public async Task<IActionResult> GetAllMonsters()
        {
            var monsters = await _context.Monsters
                .Select(m => new
                {
                    m.MonsterId,
                    m.MonsterName,
                    m.ExperienceReward,
                    TotalKills = _context.MonsterKills
                        .Where(mk => mk.MonsterId == m.MonsterId)
                        .Sum(mk => mk.Quantity)
                })
                .OrderBy(m => m.MonsterName)
                .ToListAsync();

            return Ok(monsters);
        }

        // GET: api/GameInfo/quests - Danh sách tất cả quest
        [HttpGet("quests")]
        public async Task<IActionResult> GetAllQuests()
        {
            var quests = await _context.Quests
                .Select(q => new
                {
                    q.QuestId,
                    q.QuestName,
                    q.Description,
                    q.ExperienceReward,
                    CompletionCount = _context.PlayerQuests
                        .Count(pq => pq.QuestId == q.QuestId && pq.Status == "Completed")
                })
                .OrderBy(q => q.QuestName)
                .ToListAsync();

            return Ok(quests);
        }

        // GET: api/GameInfo/vehicles - Danh sách tất cả vehicles
        [HttpGet("vehicles")]
        public async Task<IActionResult> GetAllVehicles()
        {
            var vehicles = await _context.Vehicles
                .OrderBy(v => v.PurchaseValue)
                .Select(v => new
                {
                    v.VehicleId,
                    v.VehicleName,
                    v.Description,
                    v.PurchaseValue
                })
                .ToListAsync();

            return Ok(vehicles);
        }
    }
}
