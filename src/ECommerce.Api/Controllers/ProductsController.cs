
using Microsoft.AspNetCore.Mvc;
using ECommerce.Application.Services.Products;
using ECommerce.Application.DTOs.Products;
using ECommerce.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace ECommerce.Api.Controllers;


[ApiController]
    [Route("api/[controller]")]
	[Authorize]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }
		
		/// <summary>
		/// Retrieves a list of all products available in the system.
		/// </summary>
		/// <remarks>
		/// This endpoint returns all products without pagination.
		/// Use filtering or pagination on the service layer if required.
		/// </remarks>
		/// <returns>
		/// A collection of product records.
		/// </returns>
		/// <response code="200">Products retrieved successfully.</response>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var products = await _productService.GetAllProductsAsync();
            return Ok(products);
        }
		
		
		/// <summary>
		/// Retrieves a paginated list of products.
		/// </summary>
		/// <param name="parameters">
		/// Query parameters used for pagination, filtering, and sorting such as page number,
		/// page size, search term, sort field, and sort direction.
		/// </param>
		/// <returns>
		/// Returns a paginated collection of products including pagination metadata.
		/// </returns>
		/// <response code="200">Returns the paginated list of products.</response>
		/// <response code="400">If the query parameters are invalid.</response>
		[HttpGet("paged")]
		public async Task<IActionResult> GetPaged([FromQuery] ProductQueryParametersDto parameters)
		{
			var result = await _productService.GetPaginatedProductsAsync(parameters);
			return Ok(result);
		}

        /// <summary>
		/// Retrieves a specific product by its unique identifier.
		/// </summary>
		/// <param name="id">The unique ID of the product.</param>
		/// <returns>
		/// A product matching the given ID.
		/// </returns>
		/// <response code="200">Product retrieved successfully.</response>
		/// <response code="404">No product was found with the specified ID.</response>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
                return NotFound($"Product with ID {id} not found.");

            return Ok(product);
        }

        /// <summary>
		/// Creates a new product and adds it to the system.
		/// </summary>
		/// <param name="request">The product data needed to create a new record.</param>
		/// <returns>
		/// The newly created product, including its generated ID.
		/// </returns>
		/// <response code="201">Product created successfully.</response>
		/// <response code="400">The provided product data is invalid.</response>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreateProductDto dto)
        {
	        var result = await _productService.CreateProductAsync(dto);
	        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        /// <summary>
		/// Updates an existing product with new values.
		/// </summary>
		/// <param name="id">The ID of the product to update.</param>
		/// <param name="request">The updated product data.</param>
		/// <returns>
		/// A success message or an error if the product does not exist.
		/// </returns>
		/// <response code="200">Product updated successfully.</response>
		/// <response code="400">Invalid data was provided for the update.</response>
		/// <response code="404">No product was found with the specified ID.</response>
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateProductDto dto)
        {
	        var sellerId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
	        var product = await _productService.UpdateProductAsync(id, dto, sellerId);
            if (product == null)
                return NotFound($"Product with ID {id} not found.");

            return Ok(product);
        }

        /// <summary>
		/// Deletes a product from the system.
		/// </summary>
		/// <param name="id">The ID of the product to delete.</param>
		/// <returns>
		/// A success message confirming deletion.
		/// </returns>
		/// <response code="200">Product deleted successfully.</response>
		/// <response code="404">No product was found with the specified ID.</response>
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _productService.DeleteProductAsync(id);
            if (!result)
                return NotFound($"Product with ID {id} not found.");

            return NoContent();
        }
    }