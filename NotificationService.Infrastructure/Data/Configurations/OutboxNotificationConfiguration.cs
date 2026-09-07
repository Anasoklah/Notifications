using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NotificationService.Core.Entities;
using NotificationService.Core.Enums;

namespace NotificationService.Infrastructure.Data.Configurations
{
  public class OutboxNotificationConfiguration : IEntityTypeConfiguration<OutboxNotification>
{
    public void Configure(EntityTypeBuilder<OutboxNotification> builder)
    {
        builder.ToTable("OutboxNotifications");

        builder.HasKey(n => n.Id);

        builder.Property(n => n.Id)
            .HasDefaultValueSql("gen_random_uuid()");      

        builder.Property(n => n.TargetUserId)
            .HasMaxLength(256);

        builder.Property(n => n.IsBroadcast)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(n => n.BodyLocalized)
            .HasMaxLength(512);

        builder.Property(n => n.BodyLocalized)
            .HasMaxLength(2048);

        builder.Property(n => n.Data)
            .HasColumnType("text");                        

        builder.Property(n => n.Status)
            .IsRequired()
            .HasDefaultValue(NotificationStatus.Pending)
            .HasConversion<string>();

        builder.Property(n => n.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("now()");                  

        builder.Property(n => n.LockedBy)
            .HasMaxLength(256);

        builder.Property(n => n.RetryCount)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(n => n.MaxRetries)
            .IsRequired()
            .HasDefaultValue(3);

        builder.Property(n => n.LastError)
            .HasColumnType("text");                        

        builder.HasIndex(n => new { n.Status, n.ScheduledAt, n.CreatedAt });
    }
}
}