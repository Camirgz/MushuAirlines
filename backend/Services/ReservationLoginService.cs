using backend.Model;
using backend.Repositories;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace backend.Services
{
    public class ReservationLoginService : IReservationLoginService
    {
        private readonly IReservationLoginRepository repository;

        private readonly string jwtKey;

        public ReservationLoginService(IReservationLoginRepository repository, IConfiguration configuration)
        {
            this.repository = repository;
            jwtKey = configuration["Jwt:Key"];
        }

        public string ReservationLogin(ReservationLoginModel model)
        {
            if (!repository.ReservationExists(model))
                return "Reserva no encontrada";

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey));

            var credentials = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>()
            {
                new Claim("ReservationCode",model.ReservationCode),
                new Claim("FirstName",model.FirstName),
                new Claim("LastName",model.LastName),
            };

            var token = new JwtSecurityToken(
                claims:claims,
                expires:DateTime.Now.AddMinutes(30),
                signingCredentials:credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}