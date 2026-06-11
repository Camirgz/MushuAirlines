using backend.Model;
using backend.Repositories;
using BCrypt.Net;
using System.Text.RegularExpressions;

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
            int newId = 0;
            try
            {
                if (string.IsNullOrWhiteSpace(model.FirstName))
                    return "Error: Nombre requerido";

                if (string.IsNullOrWhiteSpace(model.LastName))
                    return "Error: Apellido requerido";

                if (string.IsNullOrWhiteSpace(model.Ssn))
                    return "Error: Cédula requerida";

                if (string.IsNullOrWhiteSpace(model.Nationality))
                    return "Error: Nacionalidad requerida";

                if (string.IsNullOrWhiteSpace(model.Email))
                    return "Error: Email requerido";

                if (string.IsNullOrWhiteSpace(model.WorkSchedule))
                    return "Error: Horario requerido";
        
                string workScheduleRegexPattern = @"^([a-zA-Z]+-[a-zA-Z]+) (0?[1-9]|1[0-2])(:[0-5][0-9])?(am|AM|PM|pm)\s+a\s+(0?[1-9]|1[0-2])(:[0-5][0-9])?(AM|pm|am|pm)$";
                var workScheduleRegex = new Regex(workScheduleRegexPattern);
                if (!workScheduleRegex.IsMatch(model.WorkSchedule))
                    return "Error: Horario inválido";
                if (string.IsNullOrWhiteSpace(model.Permissions))
                    return "Error: Nota de los Permisos requeridos";
            
                var emailRegex = new Regex(@"^[^\s@]+@[^\s@]+\.[^\s@]+$");
                if (!emailRegex.IsMatch(model.Email))
                    return "Error: Email inválido";

                if (model.Salary == null)
                    return "Error: Salario requerido";
                if (model.Salary < 0)
                    return "Error: Salario inválido";

                if (pendingAccountRepository.EmailExists(model.Email) ||
                    pendingAccountRepository.PendingEmailExists(model.Email))
                {
                    return "Error: El correo ya está en uso o tiene invitación pendiente";
                }
                // create user with the data from the pending account
                newId = pendingAccountRepository.CreateUser();
                // create person with the data from the pending account and the id of the user
                pendingAccountRepository.CreatePerson(model, newId);
                // create employee with the data from the pending account and the id of the user
                pendingAccountRepository.CreateEmployee(model, newId);
                // admin or oper
                if (model.Role == "Administrator")
                {
                    pendingAccountRepository.CreateAdministrator(newId);
                }
                else
                {
                    pendingAccountRepository.CreateOperator(newId);
                }
                // generate a token for the user to complete the registration
                string token = Guid.NewGuid().ToString();
                // save the token and the email in the pending account table
                model.EmployeeId = newId;
                model.VerificationToken = token;
                model.IsVerified = false;

                // save the pending account with the token and the email
                pendingAccountRepository.SaveInvitation(model);
                // send email with the token to the user
                emailService.SendInvitationEmail(model.Email, token);


                result = "Invitation created successfully";
            }
            catch (Exception ex)
            {
                // if there was an error, we delete the user that was created
                if (newId > 0)
                {
                    pendingAccountRepository.DeletePendingByEmployeeId(newId); // opcional
                    pendingAccountRepository.DeleteUserCascade(newId);
                }
                result = "Error: " + ex.Message;
            }

            return result;
        }
       public string CompleteRegister(CompleteRegisterModel model)
        {
            var result = string.Empty;

            try
            {
                // we get the pending account with the token
                var pending = pendingAccountRepository.GetByToken(model.Token);
                // if there is no pending account with that token, we return an error
                if (pending == null)
                {
                    return "Invalid or expired token";
                }
                // regex to validate the password
                var passwordRegex = new Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*\W).{8,}$");
                if (!passwordRegex.IsMatch(model.Password))
                    return "Error: Contraseña débil";
                // we hash the password 
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(model.Password);
                // we create the account for the employee with the email and the hashed password
                pendingAccountRepository.CreateAccountEmployee(
                    pending.EmployeeId, 
                    pending.Email,
                    hashedPassword
                );
                // we mark the pending account as verified
                pendingAccountRepository.MarkAsVerified(model.Token);

                result = "Employee account created successfully";
            }
            catch (Exception ex)
            {
                result = "Error: " + ex.Message;
            }

            return result;
        }
    }
}