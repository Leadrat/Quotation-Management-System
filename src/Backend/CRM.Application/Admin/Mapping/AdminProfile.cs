using AutoMapper;
using CRM.Application.Admin.DTOs;
using CRM.Domain.Admin;

namespace CRM.Application.Admin.Mapping;

public class AdminProfile : Profile
{
    public AdminProfile()
    {
        // SystemSettings mappings can be added here if needed
        // For now, we're using direct DTO construction
    }
}

