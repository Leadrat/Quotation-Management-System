using System;

namespace CRM.Shared.Exceptions
{
    public class AccountLockedException : Exception
    {
        public AccountLockedException() : base("Account is locked due to failed attempts") { }
    }
}
