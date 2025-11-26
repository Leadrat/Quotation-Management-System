using AutoMapper;
using CRM.Application.DocumentTemplates.Dtos;
using CRM.Domain.Entities;

namespace CRM.Application.Mapping
{
    public class DocumentTemplateProfile : Profile
    {
        public DocumentTemplateProfile()
        {
            // QuotationTemplate -> DocumentTemplateDto
            CreateMap<QuotationTemplate, DocumentTemplateDto>()
                .ForMember(dest => dest.FileSizeBytes, opt => opt.MapFrom(src => src.FileSize))
                .ForMember(dest => dest.OwnerUserName, opt => opt.MapFrom(src => src.OwnerUser != null ? src.OwnerUser.GetFullName() : string.Empty))
                .ForMember(dest => dest.Visibility, opt => opt.MapFrom(src => src.Visibility.ToString()));
        }
    }
}

