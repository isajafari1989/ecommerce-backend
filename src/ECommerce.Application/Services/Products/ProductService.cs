

using System.Collections.Generic;
using System.Threading.Tasks;
using ECommerce.Application.DTOs.Products;
using ECommerce.Domain.Entities;
using ECommerce.Application.Interfaces;
using ECommerce.Application.Repositories.Products;
using ECommerce.Application.Repositories.Users;
using ECommerce.Application.Common.Exceptions;
using System.Linq;
using AutoMapper;
using System;

namespace ECommerce.Application.Services.Products;

public class ProductService : IProductService
    {
        private readonly IProductRepository _repository;
		private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUser;
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;

        public ProductService(IProductRepository repository, IMapper mapper, ICurrentUserService currentUser, IUserRepository userRepository, IUnitOfWork unitOfWork)
        {
            _repository = repository;
			_mapper = mapper;
            _currentUser = currentUser;
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<ProductDto>> GetAllProductsAsync()
        {
            var products = await _repository.GetAllAsync();

            return _mapper.Map<IEnumerable<ProductDto>>(products);
        }

        public async Task<PaginatedProductListDto> GetPaginatedProductsAsync(ProductQueryParametersDto parameters)
        {
            var paginatedResult = await _repository.GetPaginatedAsync(parameters);

            return new PaginatedProductListDto
            {
                PageNumber= paginatedResult.PageNumber,
                PageSize= paginatedResult.PageSize,
                TotalItems= paginatedResult.TotalItems,
                TotalPages= paginatedResult.TotalPages,
                Items= _mapper.Map<List<ProductDto>>(paginatedResult.Items)
            };
        }
        public async Task<ProductDto?> GetProductByIdAsync(int id)
        {
            var product = await _repository.GetByIdAsync(id);
            if (product == null) return null;

            return _mapper.Map<ProductDto>(product);
        }

        public async Task<ProductDto> CreateProductAsync(CreateProductDto dto)
        {
            var sellerId = _currentUser.UserId
                           ?? throw new UnauthorizedAccessException("User is not authenticated.");

            // 1️⃣ Validate seller exists
            var sellerExists = await _userRepository.ExistsAsync(sellerId);
            if (!sellerExists)
                throw new BadRequestException("Seller does not exist.");

            // 2️⃣ Validate SKU uniqueness
            var skuExists = await _repository.SkuExistsAsync(dto.SKU);
            if (skuExists)
                throw new ConflictException($"SKU '{dto.SKU}' already exists.");

            // 3️⃣ Validate price
            if (dto.Price <= 0)
                throw new BadRequestException("Price must be greater than zero.");

            // 4️⃣ Validate stock
            if (dto.Stock < 0)
                throw new BadRequestException("Stock cannot be negative.");

            var product = _mapper.Map<Product>(dto);
            product.SellerId = sellerId;

            await _repository.AddAsync(product);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<ProductDto>(product);
        }
        public async Task<ProductDto?> UpdateProductAsync(int id, UpdateProductDto dto, Guid sellerId)
        {
            var product = await _repository.GetByIdAsync(id);
            if (product == null)
                throw new NotFoundException($"Product with ID {id} not found.");
            
            if (product.SellerId != sellerId)
                throw new UnauthorizedAccessException("You do not own this product.");

            _mapper.Map(dto, product);

            await _repository.UpdateAsync(product);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<ProductDto>(product);
        }

        public async Task<bool> DeleteProductAsync(int id)
        {
            var product = await _repository.GetByIdAsync(id);
            if (product == null)
                throw new NotFoundException($"Product with ID {id} not found.");

            await _repository.DeleteAsync(product);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }
    }
