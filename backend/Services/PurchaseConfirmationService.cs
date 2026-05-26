using backend.Interfaces;
using backend.Model;
using System.Text.RegularExpressions;

namespace backend.Services
{
    public class PurchaseConfirmationService
    {
        private readonly IPurchaseConfirmationRepository repository;
        private readonly IEmailPurchaseService emailPurchaseService;
        public PurchaseConfirmationService(IPurchaseConfirmationRepository repository,IEmailPurchaseService emailPurchaseService)
        {
            this.repository = repository;
            this.emailPurchaseService = emailPurchaseService;
        }
        public PurchaseConfirmationModel SendConfirmation(int purchaseId)
        {
            var purchase = repository.GetPurchase(purchaseId);
            if (purchase == null)
            {
                throw new Exception("Compra no encontrada.");
            }
            var emailRegex =new Regex(@"^[^\s@]+@[^\s@]+\.[^\s@]+$");
            if (!emailRegex.IsMatch(purchase.Email))
            {
                throw new Exception("Correo invalido.");
            }
            emailPurchaseService.SendPurchaseConfirmationEmail(purchase);
            return purchase;
        }

        public void ResendEmail(int purchaseId)
        {
            var purchase = repository.GetPurchase(purchaseId);
            if (purchase == null)
            {
                throw new Exception("Compra no encontrada.");
            }
            emailPurchaseService.SendPurchaseConfirmationEmail(purchase);
        }
    }
}