using backend.Model;
using System.Net.Mail;

namespace backend.Interfaces
{
    public interface IEmailCancellationService
    {
        void SendCancellationEmail(CancellationReservationModel model);

        MailMessage BuildCancellationEmail(CancellationReservationModel model);
    }
}