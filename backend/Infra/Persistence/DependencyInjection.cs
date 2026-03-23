using Application.Common.Interfaces;
using Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Persistence.Email;
using System;
using System.Net.Http.Headers;

namespace Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ClientControlContext>(options =>
        {
            options.UseMySql(Configuration.ConnectionString, new MySqlServerVersion(new Version(8, 0, 0)),
                mySqlOptionsAction: sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(10),
                        errorNumbersToAdd: null);
                });
        }, ServiceLifetime.Scoped);
        services.AddScoped<IClientControlContext, ClientControlContext>();
        services.Configure<SendGridEmailOptions>(configuration.GetSection("SendGrid"));
        services.AddSingleton<SendGridMailRequestFactory>();
        services.AddHttpClient<IEmailProvider, SendGridEmailProvider>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<SendGridEmailOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl);
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", options.ApiKey);
        });
        return services;
    }
}
