using DAL.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Configrations
{
    internal class AppUserConfigration : IEntityTypeConfiguration<AppUser>
    {
        public void Configure(EntityTypeBuilder<AppUser> builder)
        {
            builder.HasOne(u => u.Manager)
           .WithMany(m => m.Subordinates)
           .HasForeignKey(u => u.ManagerId)
           .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
