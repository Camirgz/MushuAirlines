using backend.DTOs;
using backend.Interfaces;
using System.Data;
using System.Data.SqlClient;

namespace backend.Repositories
{
    public class MonthlyIncomeReportRepository : IMonthlyIncomeReportRepository
    {
        private readonly string _connectionString;

        public MonthlyIncomeReportRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("LoginContext")!;
        }

        public List<MonthlyIncomeReportDto> GetMonthlyIncome(MonthlyIncomeReportFilterDto filter)
        {
            var results = new List<MonthlyIncomeReportDto>();

            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand("sp_GetMonthlyIncomeReport", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.AddWithValue("@Anio",      filter.Anio.HasValue ? (object)filter.Anio.Value : DBNull.Value);
            command.Parameters.AddWithValue("@Origen",    (object?)filter.Origen    ?? DBNull.Value);
            command.Parameters.AddWithValue("@Destino",   (object?)filter.Destino   ?? DBNull.Value);
            command.Parameters.AddWithValue("@Aerolinea", (object?)filter.Aerolinea ?? DBNull.Value);

            connection.Open();
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                var row = new MonthlyIncomeReportDto
                {
                    Mes                        = reader.IsDBNull("Mes")                        ? string.Empty : reader.GetString("Mes"),
                    CantidadVuelos             = reader.IsDBNull("CantidadVuelos")             ? 0 : reader.GetInt32("CantidadVuelos"),
                    TotalPasajerosPrimeraClase = reader.IsDBNull("TotalPasajerosPrimeraClase") ? 0 : reader.GetInt32("TotalPasajerosPrimeraClase"),
                    TotalPasajerosEconomia     = reader.IsDBNull("TotalPasajerosEconomia")     ? 0 : reader.GetInt32("TotalPasajerosEconomia"),
                    TotalPasajeros             = reader.IsDBNull("TotalPasajeros")             ? 0 : reader.GetInt32("TotalPasajeros"),
                    IngresosTiquetes           = reader.IsDBNull("IngresosTiquetes")           ? 0 : reader.GetDecimal("IngresosTiquetes"),
                    IngresosMaletas            = reader.IsDBNull("IngresosMaletas")            ? 0 : reader.GetDecimal("IngresosMaletas"),
                    TotalIngresos              = reader.IsDBNull("TotalIngresos")              ? 0 : reader.GetDecimal("TotalIngresos")
                };

                results.Add(row);
            }

            return results;
        }
    }
}
