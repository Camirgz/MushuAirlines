using backend.Model;
using Dapper;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;

namespace backend.Repositories
{
    public class AirportRepository
    {
        private readonly string _connectionString;

        public AirportRepository()
        {
            var builder = WebApplication.CreateBuilder();
            _connectionString =
                builder.Configuration.GetConnectionString("AirportContext");
        }

        public List<string> GetCountries()
        {
            string query = @"
                SELECT DISTINCT Country
                FROM AirportCatalog
                ORDER BY Country;
            ";

            using var connection = new SqlConnection(_connectionString);
            return connection.Query<string>(query).ToList();
        }

        public List<string> GetCitiesByCountry(string country)
        {
            string query = @"
                SELECT City
                FROM AirportCatalog
                WHERE Country = @Country
                ORDER BY City;
            ";

            using var connection = new SqlConnection(_connectionString);
            return connection.Query<string>(query, new { Country = country }).ToList();
        }

        public bool CityBelongsToCountry(string country, string city)
        {
            string query = @"
                SELECT COUNT(1)
                FROM AirportCatalog
                WHERE Country = @Country
                  AND City = @City;
            ";

            using var connection = new SqlConnection(_connectionString);

            int count = connection.ExecuteScalar<int>(query, new
            {
                Country = country,
                City = city
            });

            return count > 0;
        }

        public bool AirportCodeExists(string code)
        {
            string query = @"
                SELECT COUNT(1)
                FROM Airport
                WHERE UPPER(Code) = UPPER(@Code);
            ";

            using var connection = new SqlConnection(_connectionString);
            int count = connection.ExecuteScalar<int>(query, new { Code = code });

            return count > 0;
        }

        public bool AirportNameExists(string airportName, string country, string city)
        {
            string query = @"
                SELECT COUNT(1)
                FROM Airport
                WHERE UPPER(AirportName) = UPPER(@AirportName)
                  AND UPPER(Country) = UPPER(@Country)
                  AND UPPER(City) = UPPER(@City);
            ";

            using var connection = new SqlConnection(_connectionString);

            int count = connection.ExecuteScalar<int>(query, new
            {
                AirportName = airportName,
                Country = country,
                City = city
            });

            return count > 0;
        }

        public void InsertAirport(AirportModel airport)
        {
            string query = @"
                INSERT INTO Airport
                (
                    Code,
                    AirportName,
                    Country,
                    City
                )
                VALUES
                (
                    @Code,
                    @AirportName,
                    @Country,
                    @City
                );
            ";

            using var connection = new SqlConnection(_connectionString);

            connection.Execute(query, new
            {
                airport.Code,
                airport.AirportName,
                airport.Country,
                airport.City
            });
        }

        public List<AirportModel> GetAirports()
        {
            string query = @"
                SELECT
                    Code,
                    AirportName,
                    Country,
                    City
                FROM Airport
                ORDER BY AirportName;
            ";

            using var connection = new SqlConnection(_connectionString);
            return connection.Query<AirportModel>(query).ToList();
        }
    }
}
