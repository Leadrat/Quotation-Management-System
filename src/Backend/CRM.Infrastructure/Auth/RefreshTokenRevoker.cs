using System;
using System.Linq;
using System.Threading.Tasks;
using CRM.Application.Auth.Services;
using CRM.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CRM.Infrastructure.Auth
{
    public class RefreshTokenRevoker : IRefreshTokenRevoker
    {
        private readonly AppDbContext _db;
        public RefreshTokenRevoker(AppDbContext db)
        {
            _db = db;
        }

        public async Task RevokeAllForUserAsync(Guid userId)
        {
            var tokens = await _db.RefreshTokens
                .Where(t => t.UserId == userId && !t.IsRevoked)
                .ToListAsync();

            foreach (var t in tokens)
            {
                t.IsRevoked = true;
                t.RevokedAt = DateTime.UtcNow;
            }

            await _db.SaveChangesAsync();
        }
    }
}
