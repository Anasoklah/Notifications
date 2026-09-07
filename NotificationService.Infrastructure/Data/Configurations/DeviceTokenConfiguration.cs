
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NotificationService.Core.Entities;

namespace NotificationService.Infrastructure.Data.Configurations
{
   public class DeviceTokenConfiguration : IEntityTypeConfiguration<DeviceToken>
{
    public void Configure(EntityTypeBuilder<DeviceToken> builder)
    {
        builder.ToTable("DeviceTokens");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id)
            .HasDefaultValueSql("gen_random_uuid()");     

        builder.Property(t => t.UserId)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(t => t.Token)
            .IsRequired()
            .HasMaxLength(512);

        builder.Property(t => t.Platform)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(t => t.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(t => t.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("now()");                  

        builder.Property(t => t.UpdatedAt)
            .IsRequired()
            .HasDefaultValueSql("now()");                  

        builder.HasIndex(t => t.Token)
            .IsUnique();

        builder.HasIndex(t => t.UserId)
            .HasFilter("\"IsActive\" = true");     
                    
    }
}
}