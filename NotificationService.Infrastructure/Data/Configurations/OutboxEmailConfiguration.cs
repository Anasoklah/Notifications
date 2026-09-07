
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NotificationService.Core.Entities;
using NotificationService.Core.Enums;

namespace NotificationService.Infrastructure.Data.Configurations;

public class OutboxEmailConfiguration : IEntityTypeConfiguration<OutboxEmail>
{
    public void Configure(EntityTypeBuilder<OutboxEmail> builder)
    {
        builder.ToTable("OutboxEmails");
           builder.HasKey(d => d.Id);

        builder.Property(d => d.Id)
            .HasDefaultValueSql("gen_random_uuid()");      
        
        builder.Property(d => d.ToEmail).IsRequired().HasMaxLength(200);
        
        builder.Property(d => d.Type).IsRequired().HasConversion<string>();
        
        builder.Property(d => d.PayloadJson).IsRequired().HasColumnType("text");
        
        builder.Property(d => d.Status)
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
