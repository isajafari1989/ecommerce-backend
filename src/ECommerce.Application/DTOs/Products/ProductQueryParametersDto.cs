namespace ECommerce.Application.DTOs.Products;

public class ProductQueryParametersDto
{
    /// <summary>
    /// Keyword used to search product name, description, or SKU.
    /// </summary>
    public string? Search { get; set; }
    
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    
    
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }



    public string? SortBy { get; set; } = "Name";
    public bool IsAscending { get; set; } = true;
}