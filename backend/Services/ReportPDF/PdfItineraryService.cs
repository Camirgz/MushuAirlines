using backend.Interfaces;
using backend.Model;
using backend.Templates;

namespace backend.Services
{
    public class PdfItineraryService : IPdfItineraryService
    {
       private readonly IQrService qrService;

        public PdfItineraryService(IQrService qrService)
        {
            this.qrService = qrService;
        }
        public byte[] GeneratePdf(PurchaseConfirmationModel model)
        {
            byte[] qr = qrService.GenerateQr(model.ReservationCode);
            return ItineraryPdfTemplate.Build(model, qr);
        }
    }
}