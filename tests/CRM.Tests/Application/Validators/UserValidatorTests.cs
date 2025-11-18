using CRM.Application.Validators;
using CRM.Domain.Entities;
using Xunit;

namespace CRM.Tests.Application.Validators;

public class UserValidatorTests
{
    private readonly UserValidator _validator = new();

    [Fact]
    public void Invalid_Email_Fails()
    {
        var user = new User { Email = "bad", PasswordHash = new string('x', 60), FirstName = "A", LastName = "B" };
        var result = _validator.Validate(user);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(User.Email));
    }

    [Fact]
    public void PhoneCode_Must_Match_Mobile_Code()
    {
        var user = new User { Email = "a@b.com", PasswordHash = new string('x', 60), FirstName = "Ab", LastName = "Cd", Mobile = "+919876543210", PhoneCode = "+1" };
        var result = _validator.Validate(user);
        Assert.False(result.IsValid);
    }

    [Fact]
    public void DeletedAt_Forces_IsActive_False()
    {
        var user = new User { Email = "a@b.com", PasswordHash = new string('x', 60), FirstName = "Ab", LastName = "Cd", DeletedAt = System.DateTime.UtcNow, IsActive = true };
        var result = _validator.Validate(user);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(User.IsActive));
    }
}
