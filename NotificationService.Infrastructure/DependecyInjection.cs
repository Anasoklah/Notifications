

using MailKit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NotificationService.Application.Interfaces.FCM;
using NotificationService.Application.Interfaces.Processors;
using NotificationService.Application.Interfaces.Smtp;
using NotificationService.Infrastructure.Processors;
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
        services.AddScoped<ISmptService, SmptSender>();
        services.AddScoped<IFCMRepository, FCMRepository>();
        services.AddScoped<ISmtpRepository, SmptRepository>();

        // Application services
        // services.AddScoped<IFCMProcessor,FCMProcessor>();
        // services.AddScoped<ISmtpProcessor,SmtpProcessor>();

        services.AddScoped<INotificationProcessor,FCMProcessor>();
        services.AddScoped<INotificationProcessor,SmtpProcessor>();

        //workers
        services.AddHostedService<FCMWorker>();
        services.AddHostedService<SmtpWorker>();

    

        return services;
        }
    }
}