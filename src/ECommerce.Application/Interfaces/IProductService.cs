using ECommerce.Application.DTOs.Products;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;


namespace ECommerce.Application.Interfaces;
public interface IProductService
    {
        Task<IEnumerable<ProductDto>> GetAllProductsAsync();
        Task<PaginatedProductListDto> GetPaginatedProductsAsync(ProductQueryParametersDto parameters);
        Task<ProductDto?> GetProductByIdAsync(int id);
        Task<ProductDto> CreateProductAsync(CreateProductDto dto);
        Task<ProductDto?> UpdateProductAsync(int id, UpdateProductDto dto, Guid sellerId);
        Task<bool> DeleteProductAsync(int id);
    }