using backend.DTOs;
using backend.Interfaces;
using System.Data;
using System.Data.SqlClient;

namespace backend.Repositories
{
    public class FlightDetailReportRepository : IFlightDetailReportRepository
    {
        private readonly string _connectionString;

        public FlightDetailReportRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("LoginContext")!;
        }

        public List<FlightDetailReportDto> GetFlightDetail(FlightDetailReportFilterDto filter)
        {
            var results = new List<FlightDetailReportDto>();

            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand("sp_GetFlightDetailReport", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.AddWithValue("@Origen",     (object?)filter.Origen     ?? DBNull.Value);
            command.Parameters.AddWithValue("@Destino",    (object?)filter.Destino    ?? DBNull.Value);
            command.Parameters.AddWithValue("@Clase",      (object?)filter.Clase      ?? DBNull.Value);
            command.Parameters.AddWithValue("@FechaDesde", filter.FechaDesde.HasValue
                ? (object)filter.FechaDesde.Value.ToDateTime(TimeOnly.MinValue)
                : DBNull.Value);
            command.Parameters.AddWithValue("@FechaHasta", filter.FechaHasta.HasValue
                ? (object)filter.FechaHasta.Value.ToDateTime(TimeOnly.MinValue)
                : DBNull.Value);

            connection.Open();
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                var row = new FlightDetailReportDto
                {
                    Fecha                 = reader.IsDBNull("Fecha")                 ? null : DateOnly.FromDateTime(reader.GetDateTime("Fecha")),
                    Origen                = reader.IsDBNull("Origen")                ? null : reader.GetString("Origen"),
                    Destino               = reader.IsDBNull("Destino")               ? null : reader.GetString("Destino"),
                    NumeroVuelo           = reader.IsDBNull("NumeroVuelo")           ? null : reader.GetInt32("NumeroVuelo"),
                    PasajerosPrimeraClase = reader.IsDBNull("PasajerosPrimeraClase") ? 0    : reader.GetInt32("PasajerosPrimeraClase"),
                    PasajerosEconomia     = reader.IsDBNull("PasajerosEconomia")     ? 0    : reader.GetInt32("PasajerosEconomia"),
                    Aerolinea             = reader.IsDBNull("Aerolinea")             ? string.Empty : reader.GetString("Aerolinea"),
                    VentaPasajeros        = reader.IsDBNull("VentaPasajeros")        ? 0    : reader.GetDecimal("VentaPasajeros"),
                    VentaEquipajes        = reader.IsDBNull("VentaEquipajes")        ? 0    : reader.GetDecimal("VentaEquipajes"),
                    TotalVenta            = reader.IsDBNull("TotalVenta")            ? 0    : reader.GetDecimal("TotalVenta")
                };

                results.Add(row);
            }

            return results;
        }
    }
}
