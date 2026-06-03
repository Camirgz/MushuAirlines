using backend.Model;

namespace backend.Services
{
    public interface IPaymentService
    {
        void ValidatePayment(PaymentModel model);
    }
}