using DAL.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Configrations
{
    internal class UserOtpConfigration : IEntityTypeConfiguration<UserOtp>
    {
        public void Configure(EntityTypeBuilder<UserOtp> builder)
        {
            builder.HasKey(c => c.Id);

            builder.HasOne(c => c.User)
                     .WithMany()
                     .HasForeignKey(c => c.UserId)
                     .OnDelete(DeleteBehavior.Cascade);

            builder.Property(c => c.OtpCode)
                   .IsRequired()
                   .HasMaxLength(6); 

            builder.Property(c => c.ExpirationTime)
                   .IsRequired();

            builder.Property(c => c.IsUsed)
                   .IsRequired()
                   .HasDefaultValue(false);
        }
    }
}
