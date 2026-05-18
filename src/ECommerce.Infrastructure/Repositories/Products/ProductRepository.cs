using ECommerce.Domain.Entities;
using ECommerce.Application.Repositories.Products;
using ECommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using ECommerce.Application.Common;
using ECommerce.Application.DTOs.Products;
using System.Linq;
using System;


namespace ECommerce.Infrastructure.Repositories.Products;

public class ProductRepository : IProductRepository
{
	private readonly AppDbContext _context;
	
	public ProductRepository(AppDbContext context)
	{
		_context = context;
	}

	public async Task<IEnumerable<Product>> GetAllAsync()
	{
		return await _context.Products
			.AsNoTracking()
			.Where(p => p.IsActive)
			.ToListAsync();
	}

	public async Task<PaginatedResult<Product>> GetPaginatedAsync(ProductQueryParametersDto parameters)
	{
		var query = _context.Products
			.AsNoTracking()
			.Where(p => p.IsActive)
			.AsQueryable();
		
		// Keyword search
		if (!string.IsNullOrWhiteSpace(parameters.Search))
		{
			var search = parameters.Search.ToLower();

			query = query.Where(p =>
				p.Name.ToLower().Contains(search) ||
				p.Description.ToLower().Contains(search) ||
				p.SKU.ToLower().Contains(search));
		}

		//Filtering
		if (parameters.MinPrice.HasValue)
			query = query.Where(p => p.Price >= parameters.MinPrice.Value);
		if (parameters.MaxPrice.HasValue)
			query = query.Where(p => p.Price <= parameters.MaxPrice.Value);

		
		//Sorting
		query = parameters.SortBy?.ToLower() switch
		{
			"name" => parameters.IsAscending ? query.OrderBy(p => p.Name) : query.OrderByDescending(p => p.Name),
			"price" => parameters.IsAscending ? query.OrderBy(p => p.Price) : query.OrderByDescending(p => p.Price),
			_ => parameters.IsAscending ? query.OrderBy(p => p.Name) : query.OrderByDescending(p => p.Name)
		};
		
		//Count total before pagination
		var totalItems = await query.CountAsync();
		
		//Pagination

		var items = await query
			.Skip((parameters.PageNumber - 1) * parameters.PageSize)
			.Take(parameters.PageSize)
			.ToListAsync();

		return new PaginatedResult<Product>
		{
			Items = items,
			PageNumber = parameters.PageNumber,
			PageSize = parameters.PageSize,
			TotalItems = totalItems,
			TotalPages = (int) Math.Ceiling(totalItems / (double) parameters.PageSize)
		};
	}

	public async Task<Product?> GetByIdAsync(int id)
	{
		return await _context.Products.FindAsync(id);
	}
	
	public async Task<bool> SkuExistsAsync(string sku)
	{
		return await _context.Products.AnyAsync(p => p.SKU == sku);
	}

	public async Task AddAsync(Product product)
	{
		_context.Products.Add(product);
		
	}

	public async Task UpdateAsync(Product product)
	{
		_context.Products.Update(product);
		
	}

	public async Task DeleteAsync(Product product)
	{
		_context.Products.Remove(product);
		
	}
}