namespace backend.DTOs
{
    public class MonthlyIncomeReportDto
    {
        public string Mes { get; set; } = string.Empty;
        public int CantidadVuelos { get; set; }
        public int TotalPasajerosPrimeraClase { get; set; }
        public int TotalPasajerosEconomia { get; set; }
        public int TotalPasajeros { get; set; }
        public decimal IngresosTiquetes { get; set; }
        public decimal IngresosMaletas { get; set; }
        public decimal TotalIngresos { get; set; }
    }

    public class MonthlyIncomeReportFilterDto
    {
        public int? Anio { get; set; }
        public string? Origen { get; set; }
        public string? Destino { get; set; }
        public string? Aerolinea { get; set; }
    }
}
