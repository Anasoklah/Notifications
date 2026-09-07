using Microsoft.EntityFrameworkCore;
using NotificationService.Core.Entities;

namespace NotificationService.Infrastructure.Data
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        public DbSet<DeviceToken> DeviceTokens { get; set; }
        public DbSet<OutboxNotification> OutboxNotifications {get; set;}
        public DbSet<NotificationDelivery>  NotificationDeliveries { get; set; }
        public DbSet<OutboxEmail> OutboxEmails { get; set; }
        public DbSet<EmailDelivery> EmailDeliveries { get; set; }

 protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
    }
}