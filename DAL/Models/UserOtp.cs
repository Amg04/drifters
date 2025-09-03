namespace DAL.Models
{
    public class UserOtp : BaseClass
    {
        public AppUser User { get; set; } = null!;
        public string UserId { get; set; }
        public string OtpCode { get; set; }
        public DateTime ExpirationTime { get; set; }
        public bool IsUsed { get; set; }
    }
}
