using backend.Model;

namespace backend.Services
{
    public interface IPaymentService
    {
        int ApprovePayment(PaymentModel model);
    }
}