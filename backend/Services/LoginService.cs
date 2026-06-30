using backend.Model;
using backend.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using backend.Interfaces;

namespace backend.Services
{
    public class LoginService : ILoginService
    {
        private readonly ILoginRepository loginRepository;
        private readonly string jwtKey;
        public LoginService(ILoginRepository loginRepository, IConfiguration configuration)
        {
            this.loginRepository = loginRepository;
            jwtKey = configuration["Jwt:Key"]!;
        }

        public string Login(LoginModel login)
        {
            var result = string.Empty;

            try
            {
                string storedHash = loginRepository.GetPasswordHash(login.Username);

                if (storedHash == null)
                {
                    return "Usuario o contraseña incorrectos";
                }

                bool isValid = BCrypt.Net.BCrypt.Verify(login.Password, storedHash);

                if (!isValid)
                {
                    return "Usuario o contraseña incorrectos";
                }
                // if is valid, generate JWT token
                // the key the secret password and we convert it to bytes and create a SymmetricSecurityKey with the bytes
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
                // we prepare the security signature of the token
                var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                
                // we get the role of the user to include it in the token
                var role = loginRepository.GetUserRole(login.Username);

                // the info we want to include in the token, in this case the username and the role of the user
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, login.Username),
                    new Claim("username", login.Username),
                    new Claim(ClaimTypes.Role, role),
                    new Claim("role", role)
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


