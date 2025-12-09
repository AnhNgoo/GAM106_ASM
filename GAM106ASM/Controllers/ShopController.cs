using GAM106ASM.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GAM106ASM.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ShopController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ShopController(AppDbContext context)
        {
            _context = context;
        }

        // Y3.3: GET: api/Shop/premium - Items với giá trị > 100 XP
        [HttpGet("premium")]
        public async Task<IActionResult> GetPremiumItems()
        {
            var premiumItems = await _context.ItemSalesSheets
                .Include(i => i.ItemType)
                .Where(i => i.PurchaseValue > 100)
                .OrderByDescending(i => i.PurchaseValue)
                .Select(i => new
                {
                    i.ItemSheetId,
                    i.ItemVersionName,
                    i.PurchaseValue,
                    i.ImageUrl,
                    ItemTypeName = i.ItemType.ItemTypeName
                })
                .ToListAsync();

            return Ok(premiumItems);
        }

        // Y3.4: GET: api/Shop/affordable/{playerId} - Items mà player đủ tiền mua
        [HttpGet("affordable/{playerId}")]
        public async Task<IActionResult> GetAffordableItems(int playerId)
        {
            var player = await _context.Players.FindAsync(playerId);
            if (player == null)
            {
                return NotFound(new { message = "Player not found" });
            }

            var affordableItems = await _context.ItemSalesSheets
                .Include(i => i.ItemType)
                .Where(i => i.PurchaseValue <= player.ExperiencePoints)
                .OrderBy(i => i.PurchaseValue)
                .Select(i => new
                {
                    i.ItemSheetId,
                    i.ItemVersionName,
                    i.PurchaseValue,
                    i.ImageUrl,
                    ItemTypeName = i.ItemType.ItemTypeName
                })
                .ToListAsync();

            return Ok(new
            {
                playerXP = player.ExperiencePoints,
                items = affordableItems
            });
        }

        // Y3.5: GET: api/Shop/search?name=kim%20cương&maxPrice=500
        [HttpGet("search")]
        public async Task<IActionResult> SearchItems(
            [FromQuery] string? name,
            [FromQuery] int? maxPrice)
        {
            var query = _context.ItemSalesSheets
                .Include(i => i.ItemType)
                .AsQueryable();

            if (!string.IsNullOrEmpty(name))
            {
                query = query.Where(i => i.ItemVersionName.ToLower().Contains(name.ToLower()));
            }

            if (maxPrice.HasValue)
            {
                query = query.Where(i => i.PurchaseValue < maxPrice.Value);
            }

            var items = await query
                .OrderBy(i => i.PurchaseValue)
                .Select(i => new
                {
                    i.ItemSheetId,
                    i.ItemVersionName,
                    i.PurchaseValue,
                    i.ImageUrl,
                    ItemTypeName = i.ItemType.ItemTypeName
                })
                .ToListAsync();

            return Ok(items);
        }

        // Y3.9: GET: api/Shop/hot - Items được mua nhiều nhất
        [HttpGet("hot")]
        public async Task<IActionResult> GetHotItems()
        {
            var hotItems = await _context.Transactions
                .Where(t => t.ItemSheetId != null && t.TransactionType == "Purchase")
                .GroupBy(t => t.ItemSheetId)
                .Select(g => new
                {
                    ItemSheetId = g.Key,
                    PurchaseCount = g.Count()
                })
                .OrderByDescending(x => x.PurchaseCount)
                .Take(10)
                .ToListAsync();

            var itemIds = hotItems.Select(h => h.ItemSheetId).ToList();

            var items = await _context.ItemSalesSheets
                .Include(i => i.ItemType)
                .Where(i => itemIds.Contains(i.ItemSheetId))
                .ToListAsync();

            var result = items.Select(item => new
            {
                item.ItemSheetId,
                item.ItemVersionName,
                item.PurchaseValue,
                item.ImageUrl,
                ItemType = item.ItemType.ItemTypeName,
                PurchaseCount = hotItems.First(h => h.ItemSheetId == item.ItemSheetId).PurchaseCount
            }).OrderByDescending(x => x.PurchaseCount);

            return Ok(result);
        }

        // POST: api/Shop/purchase - Mua item
        [HttpPost("purchase")]
        public async Task<IActionResult> PurchaseItem([FromBody] PurchaseDto dto)
        {
            var player = await _context.Players.FindAsync(dto.PlayerId);
            if (player == null)
            {
                return NotFound(new { message = "Player not found" });
            }

            var item = await _context.ItemSalesSheets.FindAsync(dto.ItemSheetId);
            if (item == null)
            {
                return NotFound(new { message = "Item not found" });
            }

            // Check if player has enough XP
            if (player.ExperiencePoints < item.PurchaseValue)
            {
                return BadRequest(new
                {
                    message = "Not enough experience points",
                    required = item.PurchaseValue,
                    current = player.ExperiencePoints
                });
            }

            // Deduct XP
            player.ExperiencePoints -= item.PurchaseValue;

            // Create transaction record
            var transaction = new Transaction
            {
                PlayerId = dto.PlayerId,
                ItemSheetId = dto.ItemSheetId,
                TransactionType = "Purchase",
                TransactionValue = item.PurchaseValue,
                TransactionTime = DateTime.UtcNow
            };

            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Purchase successful",
                transaction,
                remainingXP = player.ExperiencePoints
            });
        }

        // POST: api/Shop/purchase-vehicle - Mua vehicle
        [HttpPost("purchase-vehicle")]
        public async Task<IActionResult> PurchaseVehicle([FromBody] PurchaseVehicleDto dto)
        {
            var player = await _context.Players.FindAsync(dto.PlayerId);
            if (player == null)
            {
                return NotFound(new { message = "Player not found" });
            }

            var vehicle = await _context.Vehicles.FindAsync(dto.VehicleId);
            if (vehicle == null)
            {
                return NotFound(new { message = "Vehicle not found" });
            }

            // Check if player has enough XP
            if (player.ExperiencePoints < vehicle.PurchaseValue)
            {
                return BadRequest(new
                {
                    message = "Not enough experience points",
                    required = vehicle.PurchaseValue,
                    current = player.ExperiencePoints
                });
            }

            // Deduct XP
            player.ExperiencePoints -= vehicle.PurchaseValue;

            // Create transaction record
            var transaction = new Transaction
            {
                PlayerId = dto.PlayerId,
                VehicleId = dto.VehicleId,
                TransactionType = "Purchase",
                TransactionValue = vehicle.PurchaseValue,
                TransactionTime = DateTime.UtcNow
            };

            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Vehicle purchase successful",
                transaction,
                remainingXP = player.ExperiencePoints
            });
        }
    }

    // DTOs
    public class PurchaseDto
    {
        public int PlayerId { get; set; }
        public int ItemSheetId { get; set; }
    }

    public class PurchaseVehicleDto
    {
        public int PlayerId { get; set; }
        public int VehicleId { get; set; }
    }
}
