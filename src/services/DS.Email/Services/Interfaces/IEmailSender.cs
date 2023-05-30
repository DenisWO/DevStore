using DS.Email.Models;
using System.Threading.Tasks;

namespace DS.Email.Services.Interfaces
{
    public interface IEmailSender
    {
        Task SendEmailAsync(Message message);
    }
}
