using Application.Common.Behaviours;
using Application.Clients.Imports.Models;
using Application.Clients.Imports.Parsing;
using Application.Clients.Imports.Processing;
using Application.Clients.Imports.Queue;
using Application.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using Application.Clients.Imports;
using Application.Users.Options;
using Application.Users.Services;

namespace Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMediatR(Assembly.GetExecutingAssembly());
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(RequestValidationBehavior<,>));
        services.Configure<ClientImportOptions>(configuration.GetSection("ClientImport"));
        services.Configure<UserEmailConfirmationOptions>(configuration.GetSection("UserEmailConfirmation"));
        services.AddSingleton<ClientImportStorageService>();
        services.AddSingleton<ClientImportCsvParser>();
        services.AddSingleton<ClientImportRequestFactory>();
        services.AddScoped<UserEmailConfirmationService>();
        services.AddSingleton<IClientImportQueue, ClientImportQueue>();
        services.AddSingleton<ClientImportProcessor>();
        services.AddHostedService<ClientImportBackgroundService>();
        return services;
    }
}
