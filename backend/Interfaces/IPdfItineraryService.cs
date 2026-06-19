using backend.Model;    
namespace backend.Interfaces
{
    public interface IPdfItineraryService
    {
        byte[] GeneratePdf(PurchaseConfirmationModel model);
    }
}