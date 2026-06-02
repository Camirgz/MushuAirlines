using backend.DTOs;
using backend.Interfaces;
using backend.Model;
using Dapper;
using System.Data.SqlClient;

namespace backend.Repositories;

public class AirportRepository : IAirportRepository
{
    private readonly string _connectionString;

    public AirportRepository()
    {
        var builder = WebApplication.CreateBuilder();
        _connectionString = builder.Configuration.GetConnectionString("LoginContext")!;
    }

    public List<AirportModel> GetAirports()
    {
        const string query = @"
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

    public List<AirportCatalogDto> GetCountries()
    {
        const string query = @"
            SELECT DISTINCT
                Country,
                '' AS City
            FROM AirportCatalog
            ORDER BY Country;
        ";

        using var connection = new SqlConnection(_connectionString);

        return connection.Query<AirportCatalogDto>(query).ToList();
    }

    public List<AirportCatalogDto> GetCitiesByCountry(string country)
    {
        const string query = @"
            SELECT
                Country,
                City
            FROM AirportCatalog
            WHERE Country = @Country
            ORDER BY City;
        ";

        using var connection = new SqlConnection(_connectionString);

        return connection.Query<AirportCatalogDto>(
            query,
            new { Country = country }
        ).ToList();
    }

    public AirportModel? GetAirportByCode(string code)
    {
        const string query = @"
            SELECT
                Code,
                AirportName,
                Country,
                City
            FROM Airport
            WHERE UPPER(Code) = UPPER(@Code);
        ";

        using var connection = new SqlConnection(_connectionString);

        return connection.QuerySingleOrDefault<AirportModel>(
            query,
            new { Code = code }
        );
    }

    public bool CityBelongsToCountry(string country, string city)
    {
        const string query = @"
            SELECT COUNT(1)
            FROM AirportCatalog
            WHERE UPPER(Country) = UPPER(@Country)
              AND UPPER(City) = UPPER(@City);
        ";

        using var connection = new SqlConnection(_connectionString);

        int count = connection.ExecuteScalar<int>(
            query,
            new
            {
                Country = country,
                City = city
            }
        );

        return count > 0;
    }

    public bool AirportCodeExists(string code)
    {
        const string query = @"
            SELECT COUNT(1)
            FROM Airport
            WHERE UPPER(Code) = UPPER(@Code);
        ";

        using var connection = new SqlConnection(_connectionString);

        int count = connection.ExecuteScalar<int>(
            query,
            new { Code = code }
        );

        return count > 0;
    }

    public bool AirportNameExists(string airportName, string country, string city)
    {
        const string query = @"
            SELECT COUNT(1)
            FROM Airport
            WHERE UPPER(AirportName) = UPPER(@AirportName)
              AND UPPER(Country) = UPPER(@Country)
              AND UPPER(City) = UPPER(@City);
        ";

        using var connection = new SqlConnection(_connectionString);

        int count = connection.ExecuteScalar<int>(
            query,
            new
            {
                AirportName = airportName,
                Country = country,
                City = city
            }
        );

        return count > 0;
    }

    public bool AirportNameExistsInLocationExceptCode(
        string airportName,
        string country,
        string city,
        string excludedCode
    )
    {
        const string query = @"
            SELECT COUNT(1)
            FROM Airport
            WHERE UPPER(AirportName) = UPPER(@AirportName)
              AND UPPER(Country) = UPPER(@Country)
              AND UPPER(City) = UPPER(@City)
              AND UPPER(Code) <> UPPER(@ExcludedCode);
        ";

        using var connection = new SqlConnection(_connectionString);

        int count = connection.ExecuteScalar<int>(
            query,
            new
            {
                AirportName = airportName,
                Country = country,
                City = city,
                ExcludedCode = excludedCode
            }
        );

        return count > 0;
    }

    public void InsertAirport(AirportModel airport)
    {
        const string query = @"
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

        connection.Execute(
            query,
            new
            {
                airport.Code,
                airport.AirportName,
                airport.Country,
                airport.City
            }
        );
    }

    public bool UpdateAirportName(string code, string airportName)
    {
        const string query = @"
            UPDATE Airport
            SET AirportName = @AirportName
            WHERE UPPER(Code) = UPPER(@Code);
        ";

        using var connection = new SqlConnection(_connectionString);

        int affectedRows = connection.Execute(
            query,
            new
            {
                Code = code,
                AirportName = airportName
            }
        );

        return affectedRows > 0;
    }
}

