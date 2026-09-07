using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NotificationService.Core.Entities;
using NotificationService.Core.Enums;

namespace NotificationService.Infrastructure.Data.Configurations
{
   public class NotificationDeliveryConfiguration : IEntityTypeConfiguration<NotificationDelivery>
{
    public void Configure(EntityTypeBuilder<NotificationDelivery> builder)
    {
        builder.ToTable("NotificationDeliveries");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.Id)
            .HasDefaultValueSql("gen_random_uuid()");      

        builder.Property(d => d.Token)
            .IsRequired()
            .HasMaxLength(512);

        builder.Property(d => d.Status)
            .IsRequired()
            .HasDefaultValue(NotificationStatus.Pending)
            .HasConversion<string>();

        builder.Property(d => d.Error)
            .HasColumnType("text");                        

        builder.Property(d => d.FcmMessageId)
            .HasMaxLength(512);

        builder.Property(d => d.AttemptNumber).IsRequired().HasDefaultValue(1);

        builder.HasOne(d => d.OutboxNotification)
            .WithMany()
            .HasForeignKey(d => d.OutboxNotificationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<DeviceToken>()
            .WithMany()
            .HasForeignKey(d => d.DeviceTokenId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(d => d.AttemptNumber).IsRequired().HasDefaultValue(1);

        builder.HasIndex(d => d.OutboxNotificationId);
        builder.HasIndex(d => d.DeviceTokenId);
    }
}
}