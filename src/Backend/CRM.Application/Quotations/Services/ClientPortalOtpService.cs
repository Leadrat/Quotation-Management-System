using System;
using System.Linq;
using System.Threading.Tasks;
using CRM.Application.Common.Persistence;
using CRM.Application.Quotations.Services;
using CRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Application.Quotations.Services
{
    public interface IClientPortalOtpService
    {
        Task<string> GenerateOtpAsync(Guid accessLinkId, string clientEmail, string? ipAddress);
        Task<bool> VerifyOtpAsync(Guid accessLinkId, string clientEmail, string otpCode, string? ipAddress);
    }

    public class ClientPortalOtpService : IClientPortalOtpService
    {
        private readonly IAppDbContext _db;
        private readonly IQuotationEmailService _emailService;
        private readonly ILogger<ClientPortalOtpService> _logger;

        public ClientPortalOtpService(
            IAppDbContext db,
            IQuotationEmailService emailService,
            ILogger<ClientPortalOtpService> logger)
        {
            _db = db;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<string> GenerateOtpAsync(Guid accessLinkId, string clientEmail, string? ipAddress)
        {
            // Invalidate previous unused OTPs for this access link
            var now = DateTimeOffset.UtcNow;
            var previousOtps = await _db.ClientPortalOtps
                .Where(o => o.AccessLinkId == accessLinkId && !o.IsUsed && o.ExpiresAt >= now)
                .ToListAsync();

            foreach (var otp in previousOtps)
            {
                otp.IsUsed = true;
            }

            // Generate 6-digit OTP
            var random = new Random();
            var otpCode = random.Next(100000, 999999).ToString();

            // Hash the OTP
            var otpHash = BCrypt.Net.BCrypt.HashPassword(otpCode, workFactor: 10);

            var otpEntity = new ClientPortalOtp
            {
                OtpId = Guid.NewGuid(),
                AccessLinkId = accessLinkId,
                ClientEmail = clientEmail.ToLowerInvariant().Trim(),
                OtpCode = otpHash,
                ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(10), // OTP valid for 10 minutes
                CreatedAt = DateTimeOffset.UtcNow,
                IsUsed = false,
                Attempts = 0,
                IpAddress = ipAddress
            };

            _db.ClientPortalOtps.Add(otpEntity);
            await _db.SaveChangesAsync();

            // Send OTP via email
            try
            {
                var accessLink = await _db.QuotationAccessLinks
                    .Include(l => l.Quotation)
                    .FirstOrDefaultAsync(l => l.AccessLinkId == accessLinkId);

                if (accessLink?.Quotation != null)
                {
                    // Send OTP email using simple email method
                    if (_emailService != null)
                    {
                        var emailBody = $@"
                            <h2>Your Quotation Access Code</h2>
                            <p>Hello,</p>
                            <p>Your one-time access code for quotation <strong>{accessLink.Quotation.QuotationNumber}</strong> is:</p>
                            <h1 style=""font-size: 32px; color: #10b981; letter-spacing: 4px; margin: 20px 0;"">{otpCode}</h1>
                            <p>This code will expire in 10 minutes.</p>
                            <p>If you did not request this code, please ignore this email.</p>
                        ";

                        await _emailService.SendSimpleEmailAsync(
                            clientEmail,
                            $"Access Code for Quotation {accessLink.Quotation.QuotationNumber}",
                            emailBody);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send OTP email to {ClientEmail}", clientEmail);
                // Don't throw - OTP is already generated and saved
            }

            _logger.LogInformation("OTP generated for access link {AccessLinkId}, client {ClientEmail}", accessLinkId, clientEmail);

            return otpCode; // Return plain OTP for logging/debugging (not stored in DB)
        }

        public async Task<bool> VerifyOtpAsync(Guid accessLinkId, string clientEmail, string otpCode, string? ipAddress)
        {
            var otpEntity = await _db.ClientPortalOtps
                .Where(o => o.AccessLinkId == accessLinkId 
                    && o.ClientEmail == clientEmail.ToLowerInvariant().Trim()
                    && !o.IsUsed)
                .OrderByDescending(o => o.CreatedAt)
                .FirstOrDefaultAsync();

            if (otpEntity == null)
            {
                _logger.LogWarning("OTP not found for access link {AccessLinkId}, client {ClientEmail}", accessLinkId, clientEmail);
                return false;
            }

            if (otpEntity.IsExpired())
            {
                _logger.LogWarning("OTP expired for access link {AccessLinkId}, client {ClientEmail}", accessLinkId, clientEmail);
                otpEntity.IsUsed = true;
                await _db.SaveChangesAsync();
                return false;
            }

            if (otpEntity.Attempts >= 5)
            {
                _logger.LogWarning("OTP max attempts reached for access link {AccessLinkId}, client {ClientEmail}", accessLinkId, clientEmail);
                otpEntity.IsUsed = true;
                await _db.SaveChangesAsync();
                return false;
            }

            // Verify OTP
            otpEntity.Attempts++;
            var isValid = BCrypt.Net.BCrypt.Verify(otpCode, otpEntity.OtpCode);

            if (isValid)
            {
                otpEntity.IsUsed = true;
                otpEntity.VerifiedAt = DateTimeOffset.UtcNow;
                _logger.LogInformation("OTP verified successfully for access link {AccessLinkId}, client {ClientEmail}", accessLinkId, clientEmail);
            }
            else
            {
                _logger.LogWarning("OTP verification failed for access link {AccessLinkId}, client {ClientEmail}, attempt {Attempts}", 
                    accessLinkId, clientEmail, otpEntity.Attempts);
            }

            await _db.SaveChangesAsync();
            return isValid;
        }
    }
}

