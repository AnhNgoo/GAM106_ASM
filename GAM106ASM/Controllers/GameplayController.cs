using GAM106ASM.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GAM106ASM.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GameplayController : ControllerBase
    {
        private readonly AppDbContext _context;

        public GameplayController(AppDbContext context)
        {
            _context = context;
        }

        // POST: api/Gameplay/start-session - Bắt đầu phiên chơi
        [HttpPost("start-session")]
        public async Task<IActionResult> StartGameSession([FromBody] StartSessionDto dto)
        {
            var player = await _context.Players.FindAsync(dto.PlayerId);
            if (player == null)
            {
                return NotFound(new { message = "Player not found" });
            }

            var mode = await _context.GameModes.FindAsync(dto.ModeId);
            if (mode == null)
            {
                return NotFound(new { message = "Game mode not found" });
            }

            // Check if player already has an active session
            var activeSession = await _context.PlayHistories
                .FirstOrDefaultAsync(ph => ph.PlayerId == dto.PlayerId && ph.EndTime == null);

            if (activeSession != null)
            {
                return BadRequest(new
                {
                    message = "Player already has an active session",
                    session = activeSession
                });
            }

            var playHistory = new PlayHistory
            {
                PlayerId = dto.PlayerId,
                ModeId = dto.ModeId,
                StartTime = DateTime.UtcNow
            };

            _context.PlayHistories.Add(playHistory);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Game session started",
                session = playHistory,
                mode = mode.ModeName
            });
        }

        // POST: api/Gameplay/end-session - Kết thúc phiên chơi
        [HttpPost("end-session")]
        public async Task<IActionResult> EndGameSession([FromBody] EndSessionDto dto)
        {
            var session = await _context.PlayHistories
                .Include(ph => ph.Mode)
                .FirstOrDefaultAsync(ph => ph.PlayerId == dto.PlayerId
                    && ph.ModeId == dto.ModeId
                    && ph.EndTime == null);

            if (session == null)
            {
                return NotFound(new { message = "Active session not found" });
            }

            session.EndTime = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var duration = session.EndTime.Value - session.StartTime;

            return Ok(new
            {
                message = "Game session ended",
                session,
                duration = new
                {
                    hours = (int)duration.TotalHours,
                    minutes = duration.Minutes,
                    seconds = duration.Seconds
                }
            });
        }

        // POST: api/Gameplay/kill-monster - Giết quái vật và nhận XP
        [HttpPost("kill-monster")]
        public async Task<IActionResult> KillMonster([FromBody] KillMonsterDto dto)
        {
            var player = await _context.Players.FindAsync(dto.PlayerId);
            if (player == null)
            {
                return NotFound(new { message = "Player not found" });
            }

            var monster = await _context.Monsters.FindAsync(dto.MonsterId);
            if (monster == null)
            {
                return NotFound(new { message = "Monster not found" });
            }

            // Add or update monster kill record
            var killTime = DateTime.UtcNow;
            var monsterKill = new MonsterKill
            {
                PlayerId = dto.PlayerId,
                MonsterId = dto.MonsterId,
                KillTime = killTime,
                Quantity = dto.Quantity
            };

            _context.MonsterKills.Add(monsterKill);

            // Award experience points
            int totalXP = monster.ExperienceReward * dto.Quantity;
            player.ExperiencePoints += totalXP;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = $"Killed {dto.Quantity}x {monster.MonsterName}",
                xpGained = totalXP,
                totalXP = player.ExperiencePoints,
                monsterKill
            });
        }

        // POST: api/Gameplay/gather-resource - Thu thập tài nguyên
        [HttpPost("gather-resource")]
        public async Task<IActionResult> GatherResource([FromBody] GatherResourceDto dto)
        {
            var player = await _context.Players.FindAsync(dto.PlayerId);
            if (player == null)
            {
                return NotFound(new { message = "Player not found" });
            }

            var resource = await _context.Resources.FindAsync(dto.ResourceId);
            if (resource == null)
            {
                return NotFound(new { message = "Resource not found" });
            }

            var gatheringTime = DateTime.UtcNow;
            var resourceGathering = new ResourceGathering
            {
                PlayerId = dto.PlayerId,
                ResourceId = dto.ResourceId,
                GatheringTime = gatheringTime,
                Quantity = dto.Quantity
            };

            _context.ResourceGatherings.Add(resourceGathering);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = $"Gathered {dto.Quantity}x {resource.ResourceName}",
                resourceGathering
            });
        }

        // POST: api/Gameplay/accept-quest - Nhận quest
        [HttpPost("accept-quest")]
        public async Task<IActionResult> AcceptQuest([FromBody] QuestActionDto dto)
        {
            var player = await _context.Players.FindAsync(dto.PlayerId);
            if (player == null)
            {
                return NotFound(new { message = "Player not found" });
            }

            var quest = await _context.Quests.FindAsync(dto.QuestId);
            if (quest == null)
            {
                return NotFound(new { message = "Quest not found" });
            }

            // Check if player already has this quest
            var existingQuest = await _context.PlayerQuests
                .FirstOrDefaultAsync(pq => pq.PlayerId == dto.PlayerId && pq.QuestId == dto.QuestId);

            if (existingQuest != null)
            {
                return BadRequest(new { message = "Quest already accepted" });
            }

            var playerQuest = new PlayerQuest
            {
                PlayerId = dto.PlayerId,
                QuestId = dto.QuestId,
                Status = "In Progress"
            };

            _context.PlayerQuests.Add(playerQuest);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = $"Quest '{quest.QuestName}' accepted",
                quest,
                playerQuest
            });
        }

        // POST: api/Gameplay/complete-quest - Hoàn thành quest và nhận XP
        [HttpPost("complete-quest")]
        public async Task<IActionResult> CompleteQuest([FromBody] QuestActionDto dto)
        {
            var playerQuest = await _context.PlayerQuests
                .Include(pq => pq.Quest)
                .Include(pq => pq.Player)
                .FirstOrDefaultAsync(pq => pq.PlayerId == dto.PlayerId && pq.QuestId == dto.QuestId);

            if (playerQuest == null)
            {
                return NotFound(new { message = "Quest not found or not accepted" });
            }

            if (playerQuest.Status == "Completed")
            {
                return BadRequest(new { message = "Quest already completed" });
            }

            // Update quest status
            playerQuest.Status = "Completed";
            playerQuest.CompletionTime = DateTime.UtcNow;

            // Award experience points
            playerQuest.Player.ExperiencePoints += playerQuest.Quest.ExperienceReward;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = $"Quest '{playerQuest.Quest.QuestName}' completed!",
                xpGained = playerQuest.Quest.ExperienceReward,
                totalXP = playerQuest.Player.ExperiencePoints,
                playerQuest
            });
        }

        // PUT: api/Gameplay/update-stats - Cập nhật HP/Food
        [HttpPut("update-stats")]
        public async Task<IActionResult> UpdatePlayerStats([FromBody] UpdateStatsDto dto)
        {
            var player = await _context.Players.FindAsync(dto.PlayerId);
            if (player == null)
            {
                return NotFound(new { message = "Player not found" });
            }

            if (dto.HealthBar.HasValue)
            {
                player.HealthBar = Math.Clamp(dto.HealthBar.Value, 0, 20);
            }

            if (dto.FoodBar.HasValue)
            {
                player.FoodBar = Math.Clamp(dto.FoodBar.Value, 0, 20);
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Stats updated",
                healthBar = player.HealthBar,
                foodBar = player.FoodBar
            });
        }
    }

    // DTOs
    public class StartSessionDto
    {
        public int PlayerId { get; set; }
        public int ModeId { get; set; }
    }

    public class EndSessionDto
    {
        public int PlayerId { get; set; }
        public int ModeId { get; set; }
    }

    public class KillMonsterDto
    {
        public int PlayerId { get; set; }
        public int MonsterId { get; set; }
        public int Quantity { get; set; } = 1;
    }

    public class GatherResourceDto
    {
        public int PlayerId { get; set; }
        public int ResourceId { get; set; }
        public int Quantity { get; set; } = 1;
    }

    public class QuestActionDto
    {
        public int PlayerId { get; set; }
        public int QuestId { get; set; }
    }

    public class UpdateStatsDto
    {
        public int PlayerId { get; set; }
        public int? HealthBar { get; set; }
        public int? FoodBar { get; set; }
    }
}
