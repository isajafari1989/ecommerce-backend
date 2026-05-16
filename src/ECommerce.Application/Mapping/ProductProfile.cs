using AutoMapper;
using ECommerce.Domain.Entities;
using ECommerce.Application.DTOs.Products;

namespace ECommerce.Application.Mapping;

public class ProductProfile : Profile
{
    public ProductProfile()
    {
        //Domain -> DTO
        CreateMap<Product, ProductDto>();
        
        //DTO -> Domain
        CreateMap<CreateProductDto, Product>();
        
        //Updating existing entities
        CreateMap<UpdateProductDto, Product>();

    }
}