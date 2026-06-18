using backend.Model;

namespace backend.Services
{
    public class PaymentService : IPaymentService
    {
        private const string InsufficientFundsCard = "9999 9999 9999 9999";

        public void ValidatePayment(PaymentModel model)
        {
            if (model.CardNumber == InsufficientFundsCard)
                throw new Exception("Pago rechazado: fondos insuficientes.");

            if (IsExpired(model.Expiry))
                throw new Exception("Pago rechazado: tarjeta expirada.");
        }

        private static bool IsExpired(string expiry)
        {
            var parts = expiry.Split('/');
            int month = int.Parse(parts[0]);
            int year  = 2000 + int.Parse(parts[1]);

            return year < DateTime.Today.Year
                || (year == DateTime.Today.Year && month < DateTime.Today.Month);
        }
    }
}