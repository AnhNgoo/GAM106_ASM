using GAM106ASM.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GAM106ASM.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ResourceController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ResourceController(AppDbContext context)
        {
            _context = context;
        }

        // 1. Lấy thông tin tất cả các loại tài nguyên trong game
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Resource>>> GetAllResources()
        {
            var resources = await _context.Resources.ToListAsync();
            return Ok(resources);
        }
    }
}
