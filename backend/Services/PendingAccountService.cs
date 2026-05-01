using backend.Model;
using backend.Repositories;
using BCrypt.Net;
namespace backend.Services
{
    public class PendingAccountService
    {
        private readonly PendingAccountRepository pendingAccountRepository;
        private readonly EmailService emailService;

        public PendingAccountService()
        {
            pendingAccountRepository = new PendingAccountRepository();
            emailService = new EmailService();
        }

        public string CreateInvitation(PendingAccountModel model)
        {
            var result = string.Empty;

            try
            {
                int newId = pendingAccountRepository.CreateUser();

                pendingAccountRepository.CreatePerson(model, newId);

                pendingAccountRepository.CreateEmployee(model, newId);

                if (model.Role == "Administrator")
                {
                    pendingAccountRepository.CreateAdministrator(newId);
                }
                else
                {
                    pendingAccountRepository.CreateOperator(newId);
                }

                string token = Guid.NewGuid().ToString();

                model.EmployeeId = newId;
                model.VerificationToken = token;
                model.IsVerified = false;

                pendingAccountRepository.SaveInvitation(model);

                emailService.SendInvitationEmail(model.Email, token);

                result = "Invitation created successfully";
            }
            catch (Exception ex)
            {
                result = "ERROR REAL: " + ex.Message;
            }

            return result;
        }
        public string CompleteRegister(CompleteRegisterModel model)
        {
            var result = string.Empty;

            try
            {
                var pending = pendingAccountRepository.GetByToken(model.Token);

                if (pending == null)
                {
                    return "Invalid or expired token";
                }

                // 1. Crear User
                int newId = pendingAccountRepository.CreateUser();

                // 2. Crear Person con datos reales
                pendingAccountRepository.CreatePerson(pending, newId);

                // 3. Crear Employee con datos reales
                pendingAccountRepository.CreateEmployee(pending, newId);

                // 4. Crear rol
                if (pending.Role == "Administrator")
                {
                    pendingAccountRepository.CreateAdministrator(newId);
                }
                else
                {
                    pendingAccountRepository.CreateOperator(newId);
                }

                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(model.Password);
                pendingAccountRepository.CreateAccountEmployee(
                    newId,
                    pending.Email,
                    hashedPassword
                );

                // 6. Marcar como usado
                pendingAccountRepository.MarkAsVerified(model.Token);

                result = "Employee account created successfully";
            }
            catch (Exception ex)
            {
                result = "ERROR REAL: " + ex.Message;
            }

            return result;
        }
    }
}