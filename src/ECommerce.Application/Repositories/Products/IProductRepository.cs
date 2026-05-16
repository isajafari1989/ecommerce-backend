using ECommerce.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
using ECommerce.Application.DTOs.Products;
using ECommerce.Application.Common;

namespace ECommerce.Application.Repositories.Products;

public interface IProductRepository
{
    Task<IEnumerable<Product>> GetAllAsync();
    Task<PaginatedResult<Product>> GetPaginatedAsync(ProductQueryParametersDto parameters);
    Task<Product?> GetByIdAsync(int id);
    Task AddAsync(Product product);
    Task UpdateAsync(Product product);
    Task DeleteAsync(Product product);
    Task<bool> SkuExistsAsync(string sku);

}