using DAL.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Configrations
{
    internal class CameraDetectionConfigration : IEntityTypeConfiguration<CameraDetection>
    {
        public void Configure(EntityTypeBuilder<CameraDetection> builder)
        {
            builder.HasKey(c => c.Id);

            builder.HasOne(c => c.Camera)
               .WithMany(m => m.CameraDetections)
               .HasForeignKey(c => c.CameraId)
               .OnDelete(DeleteBehavior.Cascade);

            builder.Property(c => c.Status)
                   .IsRequired()
                   .HasMaxLength(500);

            builder.Property(c => c.Crowd_density)
                   .IsRequired();

            builder.Property(c => c.Activity_type)
                   .IsRequired()
                   .HasMaxLength(500);

            builder.Property(c => c.Threshold)
                   .IsRequired();

            builder.Property(c => c.Heatmap)
                   .IsRequired()
                   .HasMaxLength(500);
        }
    }
}
