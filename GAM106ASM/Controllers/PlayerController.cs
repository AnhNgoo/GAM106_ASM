using GAM106ASM.Models;
using GAM106ASM.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GAM106ASM.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlayerController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly OtpService _otpService;
        private readonly EmailService _emailService;

        public PlayerController(AppDbContext context, OtpService otpService, EmailService emailService)
        {
            _context = context;
            _otpService = otpService;
            _emailService = emailService;
        }

        // 2. Lấy thông tin tất cả người chơi theo từng chế độ chơi
        [HttpGet("by-game-mode/{modeName}")]
        public async Task<ActionResult<IEnumerable<object>>> GetPlayersByGameMode(string modeName)
        {
            var players = await _context.PlayHistories
                .Include(ph => ph.Player)
                .Include(ph => ph.Mode)
                .Where(ph => ph.Mode.ModeName == modeName)
                .Select(ph => new
                {
                    ph.Player.PlayerId,
                    ph.Player.EmailAccount,
                    ph.Player.ExperiencePoints,
                    ph.Player.HealthBar,
                    ph.Player.FoodBar,
                    GameMode = ph.Mode.ModeName,
                    ph.StartTime,
                    ph.EndTime
                })
                .Distinct()
                .ToListAsync();

            if (!players.Any())
            {
                return NotFound($"Không tìm thấy người chơi nào cho chế độ chơi '{modeName}'");
            }

            return Ok(players);
        }

        // 8a. Yêu cầu OTP để đặt lại mật khẩu
        [HttpPost("request-password-reset")]
        [AllowAnonymous]
        public async Task<IActionResult> RequestPasswordReset([FromBody] RequestPasswordResetRequest request)
        {
            var player = await _context.Players
                .FirstOrDefaultAsync(p => p.EmailAccount == request.Email);

            if (player == null)
            {
                return NotFound($"Không tìm thấy người chơi với email {request.Email}");
            }

            // Generate OTP
            var otp = _otpService.GenerateOtp(request.Email);

            // Send OTP via email
            try
            {
                await _emailService.SendOtpEmailAsync(request.Email, otp);
            }
            catch
            {
                return StatusCode(500, new { message = "Không thể gửi email OTP. Vui lòng thử lại sau." });
            }

            return Ok(new { message = "Mã OTP đã được gửi đến email của bạn" });
        }

        // 8b. Đặt lại mật khẩu với OTP
        [HttpPost("reset-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            var player = await _context.Players
                .FirstOrDefaultAsync(p => p.EmailAccount == request.Email);

            if (player == null)
            {
                return NotFound($"Không tìm thấy người chơi với email {request.Email}");
            }

            // Validate OTP
            if (!_otpService.ValidateOtp(request.Email, request.Otp))
            {
                return BadRequest(new { message = "Mã OTP không hợp lệ hoặc đã hết hạn" });
            }

            // Update password
            player.LoginPassword = request.NewPassword;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Đặt lại mật khẩu thành công" });
        }

        // 8c. Cập nhật mật khẩu của người chơi (yêu cầu đăng nhập)
        [HttpPut("{playerId}/password")]
        [Authorize]
        public async Task<IActionResult> UpdatePlayerPassword(int playerId, [FromBody] UpdatePasswordRequest request)
        {
            var player = await _context.Players.FindAsync(playerId);

            if (player == null)
            {
                return NotFound($"Không tìm thấy người chơi với ID {playerId}");
            }

            player.LoginPassword = request.NewPassword;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Cập nhật mật khẩu thành công" });
        }

        // 10. Lấy danh sách tất cả người chơi và số lần họ đã mua hàng
        [HttpGet("purchase-count")]
        public async Task<ActionResult<IEnumerable<object>>> GetPlayersWithPurchaseCount()
        {
            var playerPurchases = await _context.Players
                .Select(p => new
                {
                    p.PlayerId,
                    p.EmailAccount,
                    p.ExperiencePoints,
                    PurchaseCount = _context.Transactions
                        .Count(t => t.PlayerId == p.PlayerId && t.TransactionType == "mua")
                })
                .ToListAsync();

            return Ok(playerPurchases);
        }
    }

    public class UpdatePasswordRequest
    {
        public string NewPassword { get; set; } = null!;
    }

    public class RequestPasswordResetRequest
    {
        public string Email { get; set; } = null!;
    }

    public class ResetPasswordRequest
    {
        public string Email { get; set; } = null!;
        public string Otp { get; set; } = null!;
        public string NewPassword { get; set; } = null!;
    }
}
