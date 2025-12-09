using GAM106ASM.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GAM106ASM.Pages.Admin
{
    public class VehiclesModel : PageModel
    {
        private readonly AppDbContext _context;

        public VehiclesModel(AppDbContext context)
        {
            _context = context;
        }

        public List<Vehicle> Vehicles { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            if (!IsAdminLoggedIn())
                return RedirectToPage("/Admin/Login");

            Vehicles = await _context.Vehicles.OrderBy(v => v.VehicleId).ToListAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAddAsync(string name, string? description, int price)
        {
            if (!IsAdminLoggedIn())
                return RedirectToPage("/Admin/Login");

            var newVehicle = new Vehicle
            {
                VehicleName = name,
                Description = description,
                PurchaseValue = price
            };

            _context.Vehicles.Add(newVehicle);
            await _context.SaveChangesAsync();

            TempData["Message"] = $"Vehicle '{name}' added successfully!";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostEditAsync(int id, string name, string? description, int price)
        {
            if (!IsAdminLoggedIn())
                return RedirectToPage("/Admin/Login");

            var vehicle = await _context.Vehicles.FindAsync(id);
            if (vehicle != null)
            {
                vehicle.VehicleName = name;
                vehicle.Description = description;
                vehicle.PurchaseValue = price;

                await _context.SaveChangesAsync();
                TempData["Message"] = $"Vehicle '{name}' updated successfully!";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            if (!IsAdminLoggedIn())
                return RedirectToPage("/Admin/Login");

            var vehicle = await _context.Vehicles.FindAsync(id);
            if (vehicle != null)
            {
                _context.Vehicles.Remove(vehicle);
                await _context.SaveChangesAsync();
                TempData["Message"] = "Vehicle deleted successfully!";
            }

            return RedirectToPage();
        }

        private bool IsAdminLoggedIn()
        {
            return HttpContext.Session.GetInt32("AdminPlayerId").HasValue;
        }
    }
}
