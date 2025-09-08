using DAL.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace DAL.Configrations
{
    internal class MonitoredEntityConfigration : IEntityTypeConfiguration<MonitoredEntity>
    {
        public void Configure(EntityTypeBuilder<MonitoredEntity> builder)
        {
            builder.HasKey(c => c.Id);

            builder.Property(c => c.EntityType)
                .HasConversion(new EnumToStringConverter<EntityTypes>()) 
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(c => c.EntityName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(c => c.Location)
                .HasMaxLength(100);

            builder.Property(c => c.UserId)
                   .IsRequired();

            builder.HasOne(c => c.User)
                   .WithMany() 
                   .HasForeignKey(c => c.UserId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
