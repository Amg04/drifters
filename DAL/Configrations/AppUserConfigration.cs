using DAL.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Configrations
{
    internal class AppUserConfigration : IEntityTypeConfiguration<AppUser>
    {
        public void Configure(EntityTypeBuilder<AppUser> builder)
        {
            builder.Property(c => c.Name)
                  .HasMaxLength(200);

            builder.Property(c => c.ImgUrl)
                 .HasMaxLength(500);

            builder.HasOne(u => u.Manager)
           .WithMany(m => m.Subordinates)
           .HasForeignKey(u => u.ManagerId)
           .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(u => u.Email)
                .IsUnique();
        }
    }
}
