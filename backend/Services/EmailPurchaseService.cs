using backend.Interfaces;
using backend.Model;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace backend.Services
{
    public class EmailPurchaseService : IEmailPurchaseService
    {
        private readonly string from;
        private readonly string password;

        public EmailPurchaseService(IConfiguration configuration)
        {
            from =configuration["EmailSettings:From"];
            password =configuration["EmailSettings:Password"];
        }

        public void SendPurchaseConfirmationEmail(PurchaseConfirmationModel model)
        {
            var client =new SmtpClient("smtp.gmail.com",587)
            {
                EnableSsl = true, Credentials = new NetworkCredential(from,password)
            };

            var body = new StringBuilder();

            body.AppendLine($"Hola {model.FullName}");
            body.AppendLine("");
            body.AppendLine("Su compra fue realizada correctamente.");
            body.AppendLine("");
            body.AppendLine($"Código de reserva: {model.ReservationCode}");
            body.AppendLine($"QR: {model.ReservationCode}");
            body.AppendLine($"Número de factura: {model.InvoiceNumber}");
            body.AppendLine("");
            body.AppendLine("INFORMACIÓN DEL CLIENTE");
            body.AppendLine($"Nombre: {model.FullName}");
            body.AppendLine($"Pasaporte: {model.PassportNumber}");
            body.AppendLine("");
            body.AppendLine("INFORMACIÓN DEL VUELO");
            body.AppendLine($"Número de vuelo: {model.FlightNumber}");
            body.AppendLine($"Tipo de avión: {model.AircraftType}");
            body.AppendLine($"Origen: {model.OriginAirport}");
            body.AppendLine($"Destino: {model.DestinationAirport}");
            body.AppendLine($"Salida: {model.DepartureDate}");
            body.AppendLine($"Llegada: {model.ArrivalDate}");
            body.AppendLine($"Escalas: {model.Layover}");
            body.AppendLine("");
            body.AppendLine("DESGLOSE DE ASIENTOS");
            foreach (var detail in model.Details)
            {
                body.AppendLine($"{detail.SeatClass}: " + $"{detail.SeatCount} " + $"Subtotal: ${detail.Subtotal}");
            }
            body.AppendLine("");
            body.AppendLine($"Cantidad total de asientos: " +$"{model.TotalSeats}");
            body.AppendLine($"Metodo de pago: " + $"{model.PaymentMethod}");
            body.AppendLine($"Total pagado: " +$"${model.TotalPaid}");
            body.AppendLine("");
            body.AppendLine("Gracias por volar con Mushu Airlines.");
            var mail = new MailMessage
            {
                From = new MailAddress(from),
                Subject = $"Confirmación de compra - " +$"{model.ReservationCode}",
                Body = body.ToString(),
                IsBodyHtml = false
            };
            mail.To.Add(model.Email);
            client.Send(mail);
        }
    }
}