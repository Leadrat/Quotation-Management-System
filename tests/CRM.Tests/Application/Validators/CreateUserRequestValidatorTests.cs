using CRM.Application.Validators;
using CRM.Shared.DTOs;
using Xunit;

namespace CRM.Tests.Application.Validators;

public class CreateUserRequestValidatorTests
{
    private readonly CreateUserRequestValidator _validator = new();

    [Fact]
    public void WeakPassword_Fails()
    {
        var req = new CreateUserRequest { Email = "a@b.com", Password = "weak", FirstName = "Ab", LastName = "Cd" };
        var result = _validator.Validate(req);
        Assert.False(result.IsValid);
    }

    [Fact]
    public void PhoneCode_Must_Match_Mobile_Code()
    {
        var req = new CreateUserRequest { Email = "a@b.com", Password = "Strong123!", FirstName = "Ab", LastName = "Cd", Mobile = "+919876543210", PhoneCode = "+1" };
        var result = _validator.Validate(req);
        Assert.False(result.IsValid);
    }
}
