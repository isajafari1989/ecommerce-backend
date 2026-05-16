using Microsoft.Extensions.DependencyInjection;
using ECommerce.Application.Repositories.Products;
using ECommerce.Application.Repositories.Users;
using ECommerce.Infrastructure.Repositories.Products;
using ECommerce.Infrastructure.Repositories.Users;
using ECommerce.Infrastructure.Services;
using ECommerce.Application.Services.Products;
using ECommerce.Application.Services.Users;
using ECommerce.Application.Interfaces;
namespace ECommerce.Infrastructure;
using ECommerce.Infrastructure.Persistence;


public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services)
        {
            
			
            services.AddScoped<IProductService, ProductService>();

            // Register Repositories
            services.AddScoped<IProductRepository, ProductRepository>();
			
			services.AddScoped<IAuthService, AuthService>();

			services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

			services.AddScoped<IUserRepository, UserRepository>();

			services.AddScoped<IUnitOfWork, UnitOfWork>();

			services.AddHttpContextAccessor();
			services.AddScoped<ICurrentUserService, CurrentUserService>();

            return services;
        }
    }