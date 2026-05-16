namespace ECommerce.Application.DTOs.Products;
using System.Collections.Generic;
using ECommerce.Application.DTOs.Products;

public class PaginatedProductListDto
{
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    
    public int TotalItems { get; set; }
    public int TotalPages { get; set; }

    public List<ProductDto> Items { get; set; } = new();
}