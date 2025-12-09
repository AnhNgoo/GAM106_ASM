using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace GAM106ASM.Services
{
    public class EmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendOtpEmailAsync(string toEmail, string otp)
        {
            var htmlBody = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; background-color: #f5f5f5;'>
                    <div style='background: linear-gradient(135deg, #4CAF50 0%, #2E7D32 100%); padding: 30px; text-align: center; border-radius: 10px 10px 0 0;'>
                        <h1 style='color: white; margin: 0; font-size: 28px;'>🎮 Minecraft Database</h1>
                    </div>
                    <div style='background: white; padding: 40px; border-radius: 0 0 10px 10px;'>
                        <h2 style='color: #2E7D32; margin-top: 0;'>Password Change Verification</h2>
                        <p style='font-size: 16px; color: #555; line-height: 1.6;'>
                            You have requested to change your password. Please use the OTP code below to complete the process:
                        </p>
                        <div style='background: #f0f0f0; border-left: 4px solid #4CAF50; padding: 20px; margin: 30px 0; text-align: center;'>
                            <span style='font-size: 32px; font-weight: bold; color: #2E7D32; letter-spacing: 5px;'>{otp}</span>
                        </div>
                        <p style='font-size: 14px; color: #999; margin-top: 30px;'>
                            ⚠️ This OTP code will expire in 5 minutes. If you did not request this change, please ignore this email.
                        </p>
                    </div>
                    <div style='text-align: center; padding: 20px; color: #999; font-size: 12px;'>
                        <p>© 2025 Minecraft Database. All rights reserved.</p>
                    </div>
                </div>
            ";

            await SendEmailAsync(toEmail, "Password Change OTP Code", htmlBody);
        }

        public async Task<bool> SendEmailAsync(string toEmail, string subject, string body)
        {
            try
            {
                var email = new MimeMessage();
                email.From.Add(MailboxAddress.Parse(_configuration["Email:From"]));
                email.To.Add(MailboxAddress.Parse(toEmail));
                email.Subject = subject;

                var builder = new BodyBuilder
                {
                    HtmlBody = body
                };
                email.Body = builder.ToMessageBody();

                using var smtp = new SmtpClient();
                await smtp.ConnectAsync(
                    _configuration["Email:SmtpServer"],
                    int.Parse(_configuration["Email:Port"]!),
                    SecureSocketOptions.StartTls
                );

                await smtp.AuthenticateAsync(
                    _configuration["Email:Username"],
                    _configuration["Email:Password"]
                );

                await smtp.SendAsync(email);
                await smtp.DisconnectAsync(true);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending email: {ex.Message}");
                throw;
            }
        }
    }
}
