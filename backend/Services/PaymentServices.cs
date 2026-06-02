using backend.Model;

namespace backend.Services
{
    public class PaymentService
    {
        public int ApprovePayment(PaymentModel model)
        {
            if (string.IsNullOrWhiteSpace(model.PaymentMethod))
            {
                throw new Exception("Debe seleccionar un método de pago.");
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
            return 1;
        }
    }
}