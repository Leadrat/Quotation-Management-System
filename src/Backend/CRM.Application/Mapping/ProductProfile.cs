using AutoMapper;
using CRM.Application.Products.DTOs;
using CRM.Domain.Entities;

namespace CRM.Application.Mapping
{
    public class ProductProfile : Profile
    {
        public ProductProfile()
        {
            CreateMap<Product, ProductDto>()
                .ForMember(d => d.CategoryName, o => o.MapFrom(s => 
                    s.Category != null ? s.Category.CategoryName : null));

            CreateMap<ProductCategory, ProductCategoryDto>()
                .ForMember(d => d.ParentCategoryName, o => o.MapFrom(s => 
                    s.ParentCategory != null ? s.ParentCategory.CategoryName : null));
        }
    }
}

