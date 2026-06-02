using backend.Model;
using Dapper;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;
using System.Collections.Generic;

namespace backend.Repositories
{
    public class AirportCreationRepository
    {
        private readonly string _connectionString;

        public AirportCreationRepository()
        {
            var builder = WebApplication.CreateBuilder();
            _connectionString =
                builder.Configuration.GetConnectionString("LoginContext");
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

        public void InsertAirport(AirportCreationModel airport)
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

        public List<AirportCreationModel> GetAirports()
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
            return connection.Query<AirportCreationModel>(query).ToList();
        }

        public List<AirportSuggestionDto> GetSuggestions(string query)
        {
            const string sql = @"
                WITH Matches AS (
                    SELECT Code, AirportName, City
                    FROM Airport
                    WHERE City        COLLATE Latin1_General_CI_AI LIKE '%' + @Query + '%'
                       OR Code        COLLATE Latin1_General_CI_AI LIKE '%' + @Query + '%'
                       OR AirportName COLLATE Latin1_General_CI_AI LIKE '%' + @Query + '%'
                ),
                CityOptions AS (
                    SELECT DISTINCT
                        'city'    AS [Type],
                        City      AS [Value],
                        City      AS [Label],
                        City      AS [City],
                        0         AS SortKey
                    FROM Matches
                ),
                AirportOptions AS (
                    SELECT
                        'airport'                          AS [Type],
                        Code                               AS [Value],
                        AirportName + ' (' + Code + ')'   AS [Label],
                        City                               AS [City],
                        1                                  AS SortKey
                    FROM Matches
                )
                SELECT [Type], [Value], [Label], [City]
                FROM (
                    SELECT * FROM CityOptions
                    UNION ALL
                    SELECT * FROM AirportOptions
                ) AS Combined
                ORDER BY [City], SortKey, [Label];";

            using var connection = new SqlConnection(_connectionString);
            return connection.Query<AirportSuggestionDto>(sql, new { Query = query }).ToList();
        }
    }
}
