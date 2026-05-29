using backend.Model;

namespace backend.Interfaces
{
    public interface IEmailPurchaseService
    {
        void SendPurchaseConfirmationEmail(PurchaseConfirmationModel model);
        void SendInvoiceEmail(PurchaseConfirmationModel model);
    }
}