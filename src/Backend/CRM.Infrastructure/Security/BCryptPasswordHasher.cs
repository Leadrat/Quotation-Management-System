using BCrypt.Net;
using CRM.Application.Common.Security;

namespace CRM.Infrastructure.Security
{
    public class BCryptPasswordHasher : IPasswordHasher
    {
        private const int DefaultWorkFactor = 12;
        public string Hash(string password, int? workFactor = null)
        {
            var wf = workFactor ?? DefaultWorkFactor;
            return BCrypt.Net.BCrypt.HashPassword(password, workFactor: wf);
        }

        public bool Verify(string hash, string password)
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
    }
}
