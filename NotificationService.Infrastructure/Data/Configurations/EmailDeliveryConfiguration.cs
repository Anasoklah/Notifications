using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NotificationService.Core.Entities;
using NotificationService.Core.Enums;

namespace NotificationService.Infrastructure.Data.Configurations;

public class EmailDeliveryConfiguration : IEntityTypeConfiguration<EmailDelivery>
{
    public void Configure(EntityTypeBuilder<EmailDelivery> builder)
    {
        builder.ToTable("EmailDeliveries");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.Id)
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(d => d.Status)
            .IsRequired()
            .HasDefaultValue(NotificationStatus.Pending)
            .HasConversion<string>();

        builder.Property(d => d.Error)
            .HasColumnType("text");

        builder.Property(d => d.AttemptNumber)
            .IsRequired()
            .HasDefaultValue(1);

        builder.HasOne(d => d.OutboxEmail)
            .WithMany()
            .HasForeignKey(d => d.OutboxEmailId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(d => d.OutboxEmailId);
    }
}
