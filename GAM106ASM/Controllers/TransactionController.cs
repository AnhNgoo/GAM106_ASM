using GAM106ASM.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GAM106ASM.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TransactionController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TransactionController(AppDbContext context)
        {
            _context = context;
        }

        // 6. Lấy thông tin tất cả các giao dịch mua item và phương tiện của một người chơi cụ thể
        [HttpGet("player/{playerId}")]
        public async Task<ActionResult<IEnumerable<object>>> GetPlayerTransactions(int playerId)
        {
            var player = await _context.Players.FindAsync(playerId);

            if (player == null)
            {
                return NotFound($"Không tìm thấy người chơi với ID {playerId}");
            }

            var transactions = await _context.Transactions
                .Include(t => t.ItemSheet)
                    .ThenInclude(i => i.ItemType)
                .Include(t => t.Vehicle)
                .Where(t => t.PlayerId == playerId)
                .OrderBy(t => t.TransactionTime)
                .Select(t => new
                {
                    t.TransactionId,
                    t.PlayerId,
                    t.TransactionTime,
                    t.TransactionValue,
                    t.TransactionType,
                    ItemInfo = t.ItemSheet != null ? new
                    {
                        t.ItemSheet.ItemSheetId,
                        t.ItemSheet.ItemVersionName,
                        ItemType = t.ItemSheet.ItemType.ItemTypeName,
                        t.ItemSheet.PurchaseValue
                    } : null,
                    VehicleInfo = t.Vehicle != null ? new
                    {
                        t.Vehicle.VehicleId,
                        t.Vehicle.VehicleName,
                        t.Vehicle.Description,
                        t.Vehicle.PurchaseValue
                    } : null
                })
                .ToListAsync();

            return Ok(transactions);
        }

        // Lấy tất cả giao dịch (helper method)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Transaction>>> GetAllTransactions()
        {
            var transactions = await _context.Transactions
                .Include(t => t.Player)
                .Include(t => t.ItemSheet)
                    .ThenInclude(i => i.ItemType)
                .Include(t => t.Vehicle)
                .OrderByDescending(t => t.TransactionTime)
                .ToListAsync();

            return Ok(transactions);
        }
    }
}
