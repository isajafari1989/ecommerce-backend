using Microsoft.Extensions.DependencyInjection;
using AutoMapper;
using System.Reflection;
using FluentValidation;



namespace ECommerce.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        //Register FluentValidation Validators
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        //Register AutoMapper Profile
        services.AddAutoMapper(Assembly.GetExecutingAssembly());
        return services;
    }
}