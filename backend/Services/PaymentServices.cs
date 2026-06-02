using backend.Interfaces;
using backend.Model;
namespace backend.Services
{
    public class PaymentService : IPaymentService
    {
        private const string InsufficientFundsCard = "9999 9999 9999 9999";

        public int ApprovePayment(PaymentModel model)
        {
            if (string.IsNullOrWhiteSpace(model.PaymentMethod))
            {
                throw new Exception("Debe seleccionar un método de pago.");
            }
            string cardNumber = model.CardNumber;     

            if (cardNumber == InsufficientFundsCard)
            {
                throw new Exception(
                    "Pago rechazado: fondos insuficientes."
                );
            }
            if (IsExpired(model.Expiry))
            {
                throw new Exception(
                    "Pago rechazado: tarjeta expirada."
                );
             }
            

            // TODO:
            // Cuando el backend de compra esté listo:
            //
            // Crear Purchase
            // Crear PurchaseDetail
            // Crear Ticket
            //
            // devolver PurchaseId real en lugar de numero 1(que es un placeholder)
            // return purchaseId;
            return 2;
        }

        private bool IsExpired(string expiry)
        {
            var parts = expiry.Split('/');

            int month = int.Parse(parts[0]);
            int year = 2000 + int.Parse(parts[1]);

            DateTime today = DateTime.Today;

            if (year < today.Year)
            {
                return true;
            }
            if (year == today.Year && month < today.Month)
            {
                return true;
            }
            return false;
        }
    }
}