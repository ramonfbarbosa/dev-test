using Application.Common.Interfaces;
using Common;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json.Serialization;
using WebApi.Filters;

namespace WebApi.Extensions;

public static class CustomExtensionsMethods
{
    public static IServiceCollection AddCustomFramework(this IServiceCollection services)
    {
        services.AddControllers(opt =>
        {
            opt.Filters.Add(typeof(ValidateModelStateAttribute));
        })
        .AddNewtonsoftJson(options =>
            options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
        )
        .AddJsonOptions(option =>
            option.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter())
        );
        services.AddValidatorsFromAssemblyContaining<IClientControlContext>();
        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.SuppressModelStateInvalidFilter = true;
        });
        services.AddCors(options =>
        {
            options.AddPolicy("CorsPolicy",
                builder => builder
                .SetIsOriginAllowed((host) => true)
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials());
        });
        services.AddSingleton(option =>
        {
            return Configuration._configuration;
        });
        return services;
    }
}
