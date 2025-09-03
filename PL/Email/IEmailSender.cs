using System.Threading.Tasks;

namespace PL.Email
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string toEmail, string subject, string body);
    }

}
