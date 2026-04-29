using backend.Model;
using backend.Repositories;

namespace backend.Services
{
    public class LoginService
    {
        private readonly LoginRepository loginRepository;

        public LoginService()
        {
            loginRepository = new LoginRepository();
        }

        public string Login(LoginModel login)
        {
            var result = string.Empty;

            try
            {
                bool isValid = loginRepository.ValidateUser(login);

                if (!isValid)
                {
                    result = "Usuario o contraseña incorrectos";
                }
            }
            catch (Exception ex)
            {
                result = "ERROR REAL: " + ex.Message;
            }
            return result;
        }
    }
}