using GAM106ASM.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GAM106ASM.Pages.Member
{
    public class DashboardModel : PageModel
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public DashboardModel(AppDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        public Player CurrentPlayer { get; set; } = null!;
        public List<Transaction> RecentTransactions { get; set; } = new();
        public List<PlayerQuest> CompletedQuests { get; set; } = new();
        public List<PlayerQuest> IncompleteQuests { get; set; } = new();
        public int TotalMonsterKills { get; set; }
        public List<PlayHistory> PlayLogs { get; set; } = new();

        [BindProperty]
        public string NewPassword { get; set; } = "";

        [BindProperty]
        public IFormFile? AvatarFile { get; set; }

        [BindProperty]
        public IFormFile? SkinFile { get; set; }

        [TempData]
        public string? Message { get; set; }

        public bool OtpSent { get; set; }

        // Use Session instead of TempData for OTP persistence
        private string? PendingPassword
        {
            get => HttpContext.Session.GetString("OTP_PendingPassword");
            set
            {
                if (value == null)
                    HttpContext.Session.Remove("OTP_PendingPassword");
                else
                    HttpContext.Session.SetString("OTP_PendingPassword", value);
            }
        }

        private string? GeneratedOtp
        {
            get => HttpContext.Session.GetString("OTP_GeneratedOtp");
            set
            {
                if (value == null)
                    HttpContext.Session.Remove("OTP_GeneratedOtp");
                else
                    HttpContext.Session.SetString("OTP_GeneratedOtp", value);
            }
        }

        private DateTime? OtpExpiry
        {
            get
            {
                var expiryString = HttpContext.Session.GetString("OTP_Expiry");
                if (string.IsNullOrEmpty(expiryString))
                    return null;
                return DateTime.Parse(expiryString);
            }
            set
            {
                if (value == null)
                    HttpContext.Session.Remove("OTP_Expiry");
                else
                    HttpContext.Session.SetString("OTP_Expiry", value.Value.ToString("O")); // ISO 8601 format
            }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var playerId = HttpContext.Session.GetInt32("MemberPlayerId");
            if (!playerId.HasValue)
            {
                return RedirectToPage("/Index");
            }

            // Check if OTP is pending
            if (!string.IsNullOrEmpty(GeneratedOtp) && OtpExpiry.HasValue && OtpExpiry.Value > DateTime.UtcNow)
            {
                OtpSent = true;
            }

            await LoadPlayerData(playerId.Value);
            return Page();
        }

        private async Task LoadPlayerData(int playerId)
        {
            CurrentPlayer = await _context.Players
                .Include(p => p.Character)
                .FirstOrDefaultAsync(p => p.PlayerId == playerId) ?? new Player();

            RecentTransactions = await _context.Transactions
                .Include(t => t.ItemSheet)
                .Include(t => t.Vehicle)
                .Where(t => t.PlayerId == playerId)
                .OrderByDescending(t => t.TransactionTime)
                .ToListAsync();

            var quests = await _context.PlayerQuests
                .Include(pq => pq.Quest)
                .Where(pq => pq.PlayerId == playerId)
                .ToListAsync();

            CompletedQuests = quests.Where(q => q.Status == "Completed").ToList();
            IncompleteQuests = quests.Where(q => q.Status != "Completed").ToList();

            TotalMonsterKills = await _context.MonsterKills
                .Where(mk => mk.PlayerId == playerId)
                .SumAsync(mk => mk.Quantity);

            PlayLogs = await _context.PlayHistories
                .Include(ph => ph.Mode)
                .Where(ph => ph.PlayerId == playerId)
                .OrderByDescending(ph => ph.StartTime)
                .Take(20) // Limit to last 20 sessions
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostSendOtpAsync(string newPassword, string confirmPassword)
        {
            var playerId = HttpContext.Session.GetInt32("MemberPlayerId");
            if (!playerId.HasValue)
            {
                return RedirectToPage("/Index");
            }

            if (string.IsNullOrWhiteSpace(newPassword) || string.IsNullOrWhiteSpace(confirmPassword))
            {
                Message = "Error: Vui lòng điền đầy đủ thông tin!";
                await LoadPlayerData(playerId.Value);
                return Page();
            }

            if (newPassword != confirmPassword)
            {
                Message = "Error: Mật khẩu xác nhận không khớp!";
                await LoadPlayerData(playerId.Value);
                return Page();
            }

            var player = await _context.Players.FindAsync(playerId.Value);
            if (player == null)
            {
                return RedirectToPage("/Index");
            }

            // Generate 6-digit OTP
            var random = new Random();
            var otp = random.Next(100000, 999999).ToString();

            // Store OTP and password temporarily (expires in 5 minutes)
            PendingPassword = newPassword;
            GeneratedOtp = otp;
            OtpExpiry = DateTime.UtcNow.AddMinutes(5);

            // Send OTP email
            try
            {
                var configuration = HttpContext.RequestServices.GetRequiredService<IConfiguration>();
                var emailService = new Services.EmailService(configuration);
                await emailService.SendOtpEmailAsync(player.EmailAccount, otp);
                Message = "Success: OTP đã được gửi đến email của bạn!";
            }
            catch (Exception ex)
            {
                Message = $"Error: Không thể gửi email. Vui lòng thử lại sau. ({ex.Message})";
                GeneratedOtp = null;
                OtpExpiry = null;
                PendingPassword = null;
                await LoadPlayerData(playerId.Value);
                return Page();
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostVerifyOtpAsync(string otpCode)
        {
            var playerId = HttpContext.Session.GetInt32("MemberPlayerId");
            if (!playerId.HasValue)
            {
                return RedirectToPage("/Index");
            }

            // Debug: Check TempData values
            if (string.IsNullOrEmpty(GeneratedOtp))
            {
                Message = "Error: Không tìm thấy OTP. Vui lòng yêu cầu OTP mới!";
                return RedirectToPage();
            }

            if (!OtpExpiry.HasValue)
            {
                Message = "Error: Không tìm thấy thời gian hết hạn OTP. Vui lòng yêu cầu OTP mới!";
                GeneratedOtp = null;
                PendingPassword = null;
                return RedirectToPage();
            }

            // Check if OTP is expired
            var now = DateTime.UtcNow;
            var timeRemaining = (OtpExpiry.Value - now).TotalSeconds;

            if (OtpExpiry.Value < now)
            {
                Message = $"Error: OTP đã hết hạn. Vui lòng yêu cầu OTP mới! (Expired: {Math.Abs(timeRemaining):F0}s ago)";
                GeneratedOtp = null;
                PendingPassword = null;
                OtpExpiry = null;
                return RedirectToPage();
            }

            // Verify OTP
            if (string.IsNullOrWhiteSpace(otpCode) || otpCode != GeneratedOtp)
            {
                Message = "Error: Mã OTP không chính xác!";
                OtpSent = true;
                await LoadPlayerData(playerId.Value);
                return Page();
            }

            // Update password
            var player = await _context.Players.FindAsync(playerId.Value);
            if (player != null && !string.IsNullOrEmpty(PendingPassword))
            {
                player.LoginPassword = PendingPassword;
                await _context.SaveChangesAsync();
                Message = "Success: Cập nhật mật khẩu thành công!";
            }

            // Clear OTP data
            GeneratedOtp = null;
            PendingPassword = null;
            OtpExpiry = null;

            return RedirectToPage();
        }

        public IActionResult OnPostCancelOtp()
        {
            GeneratedOtp = null;
            PendingPassword = null;
            OtpExpiry = null;
            Message = "Info: Đã hủy yêu cầu đổi mật khẩu.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostUpdateAvatarAsync()
        {
            var playerId = HttpContext.Session.GetInt32("MemberPlayerId");
            if (!playerId.HasValue)
            {
                return RedirectToPage("/Index");
            }

            if (AvatarFile == null || AvatarFile.Length == 0)
            {
                Message = "Error: Vui lòng chọn file ảnh!";
                await LoadPlayerData(playerId.Value);
                return Page();
            }

            // Validate file type
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var extension = Path.GetExtension(AvatarFile.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
            {
                Message = "Error: Chỉ chấp nhận file ảnh (jpg, jpeg, png, gif)!";
                await LoadPlayerData(playerId.Value);
                return Page();
            }

            // Validate file size (max 5MB)
            if (AvatarFile.Length > 5 * 1024 * 1024)
            {
                Message = "Error: File ảnh không được vượt quá 5MB!";
                await LoadPlayerData(playerId.Value);
                return Page();
            }

            try
            {
                // Create uploads directory if it doesn't exist
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "avatars");
                Directory.CreateDirectory(uploadsFolder);

                // Generate unique filename
                var uniqueFileName = $"{playerId}_{Guid.NewGuid()}{extension}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                // Save file
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await AvatarFile.CopyToAsync(fileStream);
                }

                // Update player avatar URL
                var player = await _context.Players.FindAsync(playerId.Value);
                if (player != null)
                {
                    // Delete old avatar file if exists
                    if (!string.IsNullOrEmpty(player.AvatarUrl) && !player.AvatarUrl.StartsWith("http"))
                    {
                        var oldFilePath = Path.Combine(_environment.WebRootPath, player.AvatarUrl.TrimStart('/'));
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                    }

                    player.AvatarUrl = $"/uploads/avatars/{uniqueFileName}";
                    await _context.SaveChangesAsync();
                    Message = "Success: Cập nhật avatar thành công!";
                }
            }
            catch (Exception ex)
            {
                Message = $"Error: Lỗi khi upload avatar: {ex.Message}";
                await LoadPlayerData(playerId.Value);
                return Page();
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostUpdateSkinAsync()
        {
            var playerId = HttpContext.Session.GetInt32("MemberPlayerId");
            if (!playerId.HasValue)
            {
                return RedirectToPage("/Index");
            }

            var player = await _context.Players
                .Include(p => p.Character)
                .FirstOrDefaultAsync(p => p.PlayerId == playerId);

            if (player == null || player.Character == null)
            {
                Message = "Error: Không tìm thấy character!";
                await LoadPlayerData(playerId.Value);
                return Page();
            }

            if (SkinFile == null || SkinFile.Length == 0)
            {
                Message = "Error: Vui lòng chọn file ảnh skin!";
                await LoadPlayerData(playerId.Value);
                return Page();
            }

            // Validate file type
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var extension = Path.GetExtension(SkinFile.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
            {
                Message = "Error: Chỉ chấp nhận file ảnh (jpg, jpeg, png, gif)!";
                await LoadPlayerData(playerId.Value);
                return Page();
            }

            // Validate file size (max 5MB)
            if (SkinFile.Length > 5 * 1024 * 1024)
            {
                Message = "Error: File ảnh không được vượt quá 5MB!";
                await LoadPlayerData(playerId.Value);
                return Page();
            }

            try
            {
                // Create uploads directory if it doesn't exist
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "skins");
                Directory.CreateDirectory(uploadsFolder);

                // Generate unique filename
                var uniqueFileName = $"char_{player.Character.CharacterId}_{Guid.NewGuid()}{extension}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                // Save file
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await SkinFile.CopyToAsync(fileStream);
                }

                // Delete old skin file if exists
                if (!string.IsNullOrEmpty(player.Character.Skin) && !player.Character.Skin.StartsWith("http"))
                {
                    var oldFilePath = Path.Combine(_environment.WebRootPath, player.Character.Skin.TrimStart('/'));
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }

                player.Character.Skin = $"/uploads/skins/{uniqueFileName}";
                await _context.SaveChangesAsync();
                Message = "Success: Cập nhật skin thành công!";
            }
            catch (Exception ex)
            {
                Message = $"Error: Lỗi khi upload skin: {ex.Message}";
                await LoadPlayerData(playerId.Value);
                return Page();
            }

            return RedirectToPage();
        }

        public IActionResult OnPostLogout()
        {
            HttpContext.Session.Clear();
            return RedirectToPage("/Index");
        }
    }
}
