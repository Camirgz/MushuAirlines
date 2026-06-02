using backend.Model;
using System.Net.Mail;
namespace backend.Interfaces
{
    public interface IEmailPurchaseService
    {
        void SendPurchaseConfirmationEmail(PurchaseConfirmationModel model);
        void SendInvoiceEmail(PurchaseConfirmationModel model);
        MailMessage BuildPurchaseConfirmationEmail(PurchaseConfirmationModel model);
        MailMessage BuildInvoiceEmail(PurchaseConfirmationModel model);
    }
}