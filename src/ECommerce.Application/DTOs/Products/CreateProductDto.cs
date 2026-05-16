namespace ECommerce.Application.DTOs.Products;

public class CreateProductDto
{
    public required string Name { get; set; }
    public string Description { get; set; } = string.Empty;
    public required string SKU { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
}
