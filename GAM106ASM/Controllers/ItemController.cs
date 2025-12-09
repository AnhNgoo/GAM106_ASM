using GAM106ASM.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GAM106ASM.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ItemController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ItemController(AppDbContext context)
        {
            _context = context;
        }

        // 3. Lấy tất cả các vũ khí có giá trị trên 100 điểm kinh nghiệm
        [HttpGet("weapons/expensive")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<object>>> GetExpensiveWeapons()
        {
            var expensiveWeapons = await _context.ItemSalesSheets
                .Include(i => i.ItemType)
                .Where(i => i.ItemType.ItemTypeName.ToLower().Contains("vũ khí") && i.PurchaseValue > 100)
                .Select(i => new
                {
                    i.ItemSheetId,
                    i.ItemVersionName,
                    ItemType = i.ItemType.ItemTypeName,
                    i.PurchaseValue,
                    i.ImageUrl
                })
                .ToListAsync();

            return Ok(expensiveWeapons);
        }

        // 4. Lấy thông tin các item mà người chơi có thể mua với số điểm kinh nghiệm tích lũy hiện tại
        [HttpGet("affordable/{playerId}")]
        public async Task<ActionResult<IEnumerable<object>>> GetAffordableItems(int playerId)
        {
            var player = await _context.Players.FindAsync(playerId);

            if (player == null)
            {
                return NotFound($"Không tìm thấy người chơi với ID {playerId}");
            }

            var affordableItems = await _context.ItemSalesSheets
                .Include(i => i.ItemType)
                .Where(i => i.PurchaseValue <= player.ExperiencePoints)
                .Select(i => new
                {
                    i.ItemSheetId,
                    i.ItemVersionName,
                    ItemType = i.ItemType.ItemTypeName,
                    i.PurchaseValue,
                    i.ImageUrl,
                    PlayerExperience = player.ExperiencePoints
                })
                .ToListAsync();

            return Ok(affordableItems);
        }

        // 5. Lấy thông tin các item có tên chứa từ 'kim cương' và có giá trị dưới 500 điểm kinh nghiệm
        [HttpGet("diamond-items")]
        public async Task<ActionResult<IEnumerable<object>>> GetDiamondItems()
        {
            var diamondItems = await _context.ItemSalesSheets
                .Include(i => i.ItemType)
                .Where(i => i.ItemVersionName.ToLower().Contains("kim cương") && i.PurchaseValue < 500)
                .Select(i => new
                {
                    i.ItemSheetId,
                    i.ItemVersionName,
                    ItemType = i.ItemType.ItemTypeName,
                    i.PurchaseValue,
                    i.ImageUrl
                })
                .ToListAsync();

            return Ok(diamondItems);
        }

        // 7. Thêm thông tin của một item mới
        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<ItemSalesSheet>> CreateItem([FromBody] CreateItemRequest request)
        {
            // Kiểm tra ItemType có tồn tại không
            var itemType = await _context.ItemTypes.FindAsync(request.ItemTypeId);
            if (itemType == null)
            {
                return BadRequest($"Không tìm thấy loại item với ID {request.ItemTypeId}");
            }

            var newItem = new ItemSalesSheet
            {
                ItemTypeId = request.ItemTypeId,
                ItemVersionName = request.ItemVersionName,
                PurchaseValue = request.PurchaseValue,
                ImageUrl = request.ImageUrl
            };

            _context.ItemSalesSheets.Add(newItem);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetItemById), new { id = newItem.ItemSheetId }, newItem);
        }

        // Helper method để lấy item theo ID
        [HttpGet("{id}")]
        public async Task<ActionResult<ItemSalesSheet>> GetItemById(int id)
        {
            var item = await _context.ItemSalesSheets
                .Include(i => i.ItemType)
                .FirstOrDefaultAsync(i => i.ItemSheetId == id);

            if (item == null)
            {
                return NotFound();
            }

            return Ok(item);
        }

        // 9. Lấy danh sách các item được mua nhiều nhất
        [HttpGet("most-purchased")]
        public async Task<ActionResult<IEnumerable<object>>> GetMostPurchasedItems()
        {
            var mostPurchasedItems = await _context.Transactions
                .Where(t => t.ItemSheetId != null && t.TransactionType == "mua")
                .GroupBy(t => t.ItemSheetId)
                .Select(g => new
                {
                    ItemSheetId = g.Key,
                    PurchaseCount = g.Count()
                })
                .OrderByDescending(x => x.PurchaseCount)
                .Join(_context.ItemSalesSheets.Include(i => i.ItemType),
                    stat => stat.ItemSheetId,
                    item => item.ItemSheetId,
                    (stat, item) => new
                    {
                        item.ItemSheetId,
                        item.ItemVersionName,
                        ItemType = item.ItemType.ItemTypeName,
                        item.PurchaseValue,
                        item.ImageUrl,
                        stat.PurchaseCount
                    })
                .ToListAsync();

            return Ok(mostPurchasedItems);
        }
    }

    public class CreateItemRequest
    {
        public int ItemTypeId { get; set; }
        public string ItemVersionName { get; set; } = null!;
        public int PurchaseValue { get; set; }
        public string? ImageUrl { get; set; }
    }
}
