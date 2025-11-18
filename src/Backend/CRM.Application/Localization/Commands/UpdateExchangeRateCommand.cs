using CRM.Application.Localization.Dtos;

namespace CRM.Application.Localization.Commands;

public class UpdateExchangeRateCommand
{
    public UpdateExchangeRateRequest Request { get; set; } = new();
    public Guid CreatedByUserId { get; set; }
}


