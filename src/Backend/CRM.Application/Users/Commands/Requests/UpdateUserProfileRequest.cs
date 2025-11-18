namespace CRM.Application.Users.Commands.Requests
{
    public class UpdateUserProfileRequest
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? PhoneCode { get; set; }
        public string? Mobile { get; set; }
    }
}
