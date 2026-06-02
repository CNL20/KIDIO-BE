using System.Threading.Tasks;

namespace KIDIO.Business.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string body);
    }
}