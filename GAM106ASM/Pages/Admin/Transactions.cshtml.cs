using GAM106ASM.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GAM106ASM.Pages.Admin
{
    public class TransactionsModel : PageModel
    {
        private readonly AppDbContext _context;

        public TransactionsModel(AppDbContext context)
        {
            _context = context;
        }

        public List<Transaction> Transactions { get; set; } = new();
        public List<Player> Players { get; set; } = new();
        public List<ItemSalesSheet> Items { get; set; } = new();
        public List<Vehicle> Vehicles { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            if (!IsAdminLoggedIn())
                return RedirectToPage("/Admin/Login");

            Transactions = await _context.Transactions
                .Include(t => t.Player)
                .Include(t => t.ItemSheet)
                .Include(t => t.Vehicle)
                .OrderByDescending(t => t.TransactionTime)
                .ToListAsync();

            Players = await _context.Players.ToListAsync();
            Items = await _context.ItemSalesSheets.ToListAsync();
            Vehicles = await _context.Vehicles.ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostAddAsync(int playerId, string transactionType, int? itemSheetId, int? vehicleId, int transactionValue)
        {
            if (!IsAdminLoggedIn())
                return RedirectToPage("/Admin/Login");

            var newTransaction = new Transaction
            {
                PlayerId = playerId,
                TransactionType = transactionType,
                ItemSheetId = itemSheetId,
                VehicleId = vehicleId,
                TransactionValue = transactionValue,
                TransactionTime = DateTime.Now
            };

            _context.Transactions.Add(newTransaction);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Transaction added successfully!";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            if (!IsAdminLoggedIn())
                return RedirectToPage("/Admin/Login");

            var transaction = await _context.Transactions.FindAsync(id);
            if (transaction != null)
            {
                _context.Transactions.Remove(transaction);
                await _context.SaveChangesAsync();
                TempData["Message"] = "Transaction deleted successfully!";
            }

            return RedirectToPage();
        }

        private bool IsAdminLoggedIn()
        {
            return HttpContext.Session.GetInt32("AdminPlayerId").HasValue;
        }
    }
}
