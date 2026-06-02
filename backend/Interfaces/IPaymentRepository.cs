using backend.Model;

namespace backend.Interfaces
{
    public interface IPaymentRepository
    {
        int CreatePurchase(PurchaseRecord purchase);

        void CreatePurchaseDetail(
            int purchaseId,
            SeatClassSubtotal detail
        );
    }
}