using QRCoder;
using backend.Interfaces;
namespace backend.Services
{
    // Generates QR images for check-in and tickets
    public class QrService : IQrService
    {
        public byte[] GenerateQr(string text)
        {
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            // Create QR data from reservation content
            QRCodeData qrData = qrGenerator.CreateQrCode(text,QRCodeGenerator.ECCLevel.Q);
            PngByteQRCode qrCode =new PngByteQRCode(qrData);
            // Return QR image as PNG bytes
            return qrCode.GetGraphic(20);
        }
    }
}