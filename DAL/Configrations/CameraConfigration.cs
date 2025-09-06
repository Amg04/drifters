using DAL.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Configrations
{
    internal class CameraConfigration : IEntityTypeConfiguration<Camera>
    {
        public void Configure(EntityTypeBuilder<Camera> builder)
        {
            builder.HasKey(c => c.Id);

            builder.Property(c => c.Host)
                   .IsRequired()
                   .HasMaxLength(200);

            builder.Property(c => c.Port)
                   .IsRequired();

            builder.Property(c => c.Username)
                   .HasMaxLength(100);

            builder.Property(c => c.RtspPath)
                   .IsRequired()
                   .HasMaxLength(500);

            builder.Property(c => c.PasswordEnc)
                   .IsRequired()
                   .HasMaxLength(500);

            builder.Property(c => c.HlsPublicUrl)
                   .HasMaxLength(500);

            builder.Property(c => c.HlsLocalPath)
                   .HasMaxLength(500);

            builder.Property(c => c.Status)
                   .HasMaxLength(50)
                   .HasDefaultValue("Unknown");

            builder.Property(c => c.LastHeartbeatUtc)
                   .HasColumnType("datetime2");          

            builder.Property(c => c.CameraLocation)
                  .IsRequired()
                  .HasMaxLength(500);

            builder.Property(c => c.MonitoredEntityId)
                 .IsRequired();

            builder.HasOne(c => c.MonitoredEntity)
                .WithMany(m => m.Cameras)
                .HasForeignKey(c => c.MonitoredEntityId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
