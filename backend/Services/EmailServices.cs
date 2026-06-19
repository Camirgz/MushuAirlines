using System.Net;
using System.Net.Mail;

namespace backend.Services
{
    public class EmailService
    {
        private readonly string _from;
        private readonly string _password;

        public EmailService()
        {
            // we get the email and password from the appsettings.json file
            var builder = WebApplication.CreateBuilder();
            _from = builder.Configuration["EmailSettings:From"];
            _password = builder.Configuration["EmailSettings:Password"];
        }

        public void SendInvitationEmail(string toEmail, string token)
        {
            // we create the link for the employee to complete the registration
            string link = "https://mushu-airlines.vercel.app/complete-register?token=" + token;
            // create a connection to the smtp server and send the email at the port 587 
            var client = new SmtpClient("smtp.gmail.com", 587)
            {
                //encrypt the connection with SSL
                EnableSsl = true,
                // credentials are the email and password of the sender email account in the json
                Credentials = new NetworkCredential(_from, _password)
            };

            var mail = new MailMessage
            {
                // the email is sent from the email in the json
                From = new MailAddress(_from),
                // the subject and body of the email, we include the link to complete the registration in the body
                Subject = "Bienvenido a Mushu Airlines - Completa tu registro",
                Body = $"Haz clic en el siguiente enlace para completar tu registro de empleado:\n\n{link}",
                // we specify that the body is not HTML
                IsBodyHtml = false
            };
            // the email is sent to the email of the employee that we want to invite
            mail.To.Add(toEmail);
            client.Send(mail);
            Console.WriteLine($"Invitation email sent to {toEmail} with token {token}");
        }
    }
}