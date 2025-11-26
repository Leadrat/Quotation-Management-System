using AutoMapper;
using CRM.Application.Notifications.DTOs;
using CRM.Application.Notifications.Services;

namespace CRM.Application.Notifications.Commands.Handlers;

public class CreateNotificationTemplateCommandHandler
{
    private readonly INotificationTemplateService _templateService;
    private readonly IMapper _mapper;

    public CreateNotificationTemplateCommandHandler(
        INotificationTemplateService templateService,
        IMapper mapper)
    {
        _templateService = templateService;
        _mapper = mapper;
    }

    public async Task<CreateNotificationTemplateCommandResult> HandleAsync(CreateNotificationTemplateCommand command)
    {
        var request = _mapper.Map<CreateNotificationTemplateRequest>(command);
        var template = await _templateService.CreateTemplateAsync(request);

        return new CreateNotificationTemplateCommandResult
        {
            Template = template
        };
    }
}