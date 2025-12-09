namespace GAM106ASM.Services
{
    public class OtpService
    {
        private readonly Dictionary<string, OtpData> _otpStore = new();
        private readonly object _lock = new();

        public string GenerateOtp(string email)
        {
            lock (_lock)
            {
                var otp = new Random().Next(100000, 999999).ToString();
                var expiryTime = DateTime.UtcNow.AddMinutes(5);

                _otpStore[email] = new OtpData
                {
                    Otp = otp,
                    ExpiryTime = expiryTime
                };

                // Cleanup expired OTPs
                CleanupExpiredOtps();

                return otp;
            }
        }

        public bool ValidateOtp(string email, string otp)
        {
            lock (_lock)
            {
                if (!_otpStore.ContainsKey(email))
                    return false;

                var otpData = _otpStore[email];

                if (DateTime.UtcNow > otpData.ExpiryTime)
                {
                    _otpStore.Remove(email);
                    return false;
                }

                if (otpData.Otp != otp)
                    return false;

                // OTP is valid, remove it from store (one-time use)
                _otpStore.Remove(email);
                return true;
            }
        }

        private void CleanupExpiredOtps()
        {
            var expiredKeys = _otpStore
                .Where(kvp => DateTime.UtcNow > kvp.Value.ExpiryTime)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var key in expiredKeys)
            {
                _otpStore.Remove(key);
            }
        }

        private class OtpData
        {
            public string Otp { get; set; } = null!;
            public DateTime ExpiryTime { get; set; }
        }
    }
}
