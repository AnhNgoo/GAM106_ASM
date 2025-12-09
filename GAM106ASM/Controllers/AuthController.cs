using GAM106ASM.Models;
using GAM106ASM.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GAM106ASM.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly JwtService _jwtService;

        public AuthController(AppDbContext context, JwtService jwtService)
        {
            _context = context;
            _jwtService = jwtService;
        }

        // Đăng nhập
        [HttpPost("login")]
        public async Task<ActionResult> Login([FromBody] LoginRequest request)
        {
            var player = await _context.Players
                .FirstOrDefaultAsync(p => p.EmailAccount == request.Email && p.LoginPassword == request.Password);

            if (player == null)
            {
                return Unauthorized(new { message = "Email hoặc mật khẩu không chính xác" });
            }

            // Determine role (default to "member" if Role is null)
            var role = player.Role ?? "member";

            // Generate JWT token
            var token = _jwtService.GenerateToken(player.PlayerId, player.EmailAccount, role);

            return Ok(new
            {
                message = "Đăng nhập thành công",
                token,
                player = new
                {
                    player.PlayerId,
                    player.EmailAccount,
                    player.ExperiencePoints,
                    player.HealthBar,
                    player.FoodBar,
                    player.Role,
                    player.AvatarUrl
                }
            });
        }

        // Đăng ký
        [HttpPost("register")]
        public async Task<ActionResult> Register([FromBody] RegisterRequest request)
        {
            // Check if email already exists
            var existingPlayer = await _context.Players
                .FirstOrDefaultAsync(p => p.EmailAccount == request.Email);

            if (existingPlayer != null)
            {
                return BadRequest(new { message = "Email đã được sử dụng" });
            }

            // Create new player
            var newPlayer = new Player
            {
                EmailAccount = request.Email,
                LoginPassword = request.Password,
                ExperiencePoints = 0,
                HealthBar = 100,
                FoodBar = 100,
                Role = "member" // Default role
            };

            _context.Players.Add(newPlayer);
            await _context.SaveChangesAsync();

            // Generate JWT token
            var token = _jwtService.GenerateToken(newPlayer.PlayerId, newPlayer.EmailAccount, "member");

            return Ok(new
            {
                message = "Đăng ký thành công",
                token,
                player = new
                {
                    newPlayer.PlayerId,
                    newPlayer.EmailAccount,
                    newPlayer.ExperiencePoints,
                    newPlayer.HealthBar,
                    newPlayer.FoodBar,
                    newPlayer.Role
                }
            });
        }

        // Lấy thông tin người dùng hiện tại
        [HttpGet("me")]
        public async Task<ActionResult> GetCurrentUser()
        {
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

            if (string.IsNullOrEmpty(token))
            {
                return Unauthorized(new { message = "Token không hợp lệ" });
            }

            var principal = _jwtService.ValidateToken(token);

            if (principal == null)
            {
                return Unauthorized(new { message = "Token không hợp lệ" });
            }

            var playerIdClaim = principal.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);

            if (playerIdClaim == null)
            {
                return Unauthorized(new { message = "Token không hợp lệ" });
            }

            var playerId = int.Parse(playerIdClaim.Value);
            var player = await _context.Players.FindAsync(playerId);

            if (player == null)
            {
                return NotFound(new { message = "Người chơi không tồn tại" });
            }

            return Ok(new
            {
                player.PlayerId,
                player.EmailAccount,
                player.ExperiencePoints,
                player.HealthBar,
                player.FoodBar,
                player.Role,
                player.AvatarUrl
            });
        }
    }

    public class LoginRequest
    {
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
    }

    public class RegisterRequest
    {
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
    }
}
