using CRM.Application.Validators;
using CRM.Shared.DTOs;
using FluentValidation.TestHelper;
using Xunit;

namespace CRM.Tests.Application.Validators;

public class UpdateUserRequestValidatorTests
{
    private readonly UpdateUserRequestValidator _validator = new();

    [Fact]
    public void Empty_Update_Is_Valid()
    {
        var req = new UpdateUserRequest();
        var result = _validator.TestValidate(req);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Bad_Name_Fails()
    {
        var req = new UpdateUserRequest { FirstName = "1", LastName = "@" };
        var result = _validator.TestValidate(req);
        result.ShouldHaveAnyValidationError();
    }

    [Fact]
    public void PhoneCode_Must_Match_Mobile_Code()
    {
        var req = new UpdateUserRequest { Mobile = "+919876543210", PhoneCode = "+1" };
        var result = _validator.TestValidate(req);
        result.ShouldHaveAnyValidationError();
    }
}
