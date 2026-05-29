using backend.Interfaces;
using backend.Model;
using System.IO;
using System.Net;
using backend.Templates;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;

namespace backend.Services
{
    public class EmailPurchaseService : IEmailPurchaseService
    {
        private readonly string from;
        private readonly string password;
        private readonly IQrService qrService;

        public EmailPurchaseService(IConfiguration configuration, IQrService qrService)
        {
            from =configuration["EmailSettings:From"];
            password =configuration["EmailSettings:Password"];
            this.qrService = qrService;
        }

        public void SendPurchaseConfirmationEmail(PurchaseConfirmationModel model)
        {
            var client =new SmtpClient("smtp.gmail.com",587)
            {
                EnableSsl = true, Credentials = new NetworkCredential(from,password)
            };

            // view of the email body with purchase details
            string body = PurchaseEmailTemplate.Build(model);
            
            // Qr code image as byte array from reservation code to use in the html body
            byte[] qrImage = qrService.GenerateQr(model.ReservationCode);
            MemoryStream stream = new MemoryStream(qrImage);
            LinkedResource qrResource = new LinkedResource(stream, "image/png");
            qrResource.ContentId = "qrcode";

            // create the view of the email body with the QR code as linked resource
            AlternateView htmlView = AlternateView.CreateAlternateViewFromString(body,Encoding.UTF8,MediaTypeNames.Text.Html);
            htmlView.LinkedResources.Add(qrResource);
            var mail = new MailMessage
            {
                From = new MailAddress(from),
                Subject = $"Confirmación de compra - " +$"{model.ReservationCode}",
                IsBodyHtml = true
            };
            mail.To.Add(model.Email);
            mail.AlternateViews.Add(htmlView);
            client.Send(mail);
        }
    }
}