namespace ECommerce.Application.DTOs.Products;

public class ProductDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public decimal Price { get; set; }
}

