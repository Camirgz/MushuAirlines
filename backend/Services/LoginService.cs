using backend.Model;
using backend.Repositories;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace backend.Services
{
    public class LoginService
    {
        private readonly LoginRepository loginRepository;
        private readonly string jwtKey = "MushuClaveLeo.ari,Cami,alex;dani";
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
                    return "Usuario o contraseña incorrectos";
                }
                // if is valid, generate JWT token
                // the key the secret password and we convert it to bytes and create a SymmetricSecurityKey with the bytes
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
                // we prepare the security signature of the token
                var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                
                // the info we want to include in the token
                var claims = new[]
                {
                    // include in the token who logged in
                    new Claim(ClaimTypes.Name, login.Username)
                };

                // we create the token with the info, the expiration time and the signature
                var token = new JwtSecurityToken(
                    claims: claims,
                    expires: DateTime.Now.AddMinutes(30),
                    signingCredentials: credentials
                );
                // we convert the token to a string and return it
                result = new JwtSecurityTokenHandler().WriteToken(token);
            }
            catch (Exception ex)
            {
                result = "ERROR REAL: " + ex.Message;
            }
            return result;
        }
    }
}


