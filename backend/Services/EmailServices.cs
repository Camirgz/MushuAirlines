namespace backend.Services
{
    public class EmailService
    {
        public void SendInvitationEmail(string email, string token)
        {
            string link = "http://localhost:8080/complete-register?token=" + token;

            Console.WriteLine("CORREO ENVIADO A: " + email);
            Console.WriteLine("LINK DE REGISTRO: " + link);
        }
    }
}