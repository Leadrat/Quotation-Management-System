using CRM.Shared.Helpers;
using Xunit;

namespace CRM.Tests.Shared;

public class PasswordHelperTests
{
    [Fact]
    public void Hash_And_Verify_Works()
    {
        var plain = "P@ssw0rd!";
        var hash = PasswordHelper.HashPassword(plain);
        Assert.True(PasswordHelper.VerifyPassword(plain, hash));
        Assert.False(PasswordHelper.VerifyPassword("Wrong123!", hash));
    }
}
