using backend.Interfaces;
using backend.Model;
using backend.Templates;
using System.Net;
using System.Net.Mail;

namespace backend.Services
{
    public class EmailCancellationService
        : IEmailCancellationService
    {
        private readonly string from;
        private readonly string password;

        public EmailCancellationService(IConfiguration configuration)
        {
            from = configuration["EmailSettings:From"]!;
            password = configuration["EmailSettings:Password"]!;
        }

        public void SendCancellationEmail(
            CancellationReservationModel model)
        {
            var client = new SmtpClient("smtp.gmail.com", 587)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(from, password)
            };

            client.Send(BuildCancellationEmail(model));
        }

        public MailMessage BuildCancellationEmail(
            CancellationReservationModel model)
        {
            string body = CancellationEmailTemplate.Build(model);

            var mail = new MailMessage
            {
                From = new MailAddress(from),
                Subject = $"Cancelar reserva - {model.ReservationCode}",
                Body = body,
                IsBodyHtml = true
            };

            mail.To.Add(model.Email);

            return mail;
        }
    }
}