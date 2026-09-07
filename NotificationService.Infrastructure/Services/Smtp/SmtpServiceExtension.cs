using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace NotificationService.Infrastructure.Services.Smtp;

public static class SmtpServiceExtension
{
    public static IServiceCollection AddSmptService(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<SmtpOptions>(
            configuration.GetSection("Smtp")
        );
        return services;
    }
}
