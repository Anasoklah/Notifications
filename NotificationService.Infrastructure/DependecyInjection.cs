

using MailKit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NotificationService.Core.Interfaces;
using NotificationService.Core.Services;
using NotificationService.Infrastructure.Data;
using NotificationService.Infrastructure.Repositories;
using NotificationService.Infrastructure.Services;
using NotificationService.Infrastructure.Services.Smtp;
using NotificationService.Infrastructure.Workers;

namespace NotificationService.Infrastructure
{
    public static class DependecyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {

        //settings
        services.Configure<SmtpOptions>(configuration.GetSection("Smtp"));
        
        // Services
        services.AddScoped<IFcmSender, FcmSender>();
        services.AddScoped<ISmptService, SmptService>();
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<ISmtpRepository, SmptRepository>();

        // Application services
        services.AddScoped<NotificationProcessingService>();
        services.AddScoped<EmailProcessingService>();
        
        //workers
        services.AddHostedService<FCMWorker>();
        services.AddHostedService<SmtpWorker>();

    

        return services;
        }
    }
}