namespace backend.DTOs
{
    public class FlightDetailReportDto
    {
        public DateOnly? Fecha { get; set; }
        public string? Origen { get; set; }
        public string? Destino { get; set; }
        public string? CodigoVuelo { get; set; }
        public int PasajerosPrimeraClase { get; set; }
        public int PasajerosEconomia { get; set; }
        public string Aerolinea { get; set; } = string.Empty;
        public decimal VentaPasajeros { get; set; }
        public decimal VentaEquipajes { get; set; }
        public decimal TotalVenta { get; set; }
    }

    public class FlightDetailReportFilterDto
    {
        public string? Origen { get; set; }
        public string? Destino { get; set; }
        public string? Clase { get; set; }
        public DateOnly? FechaDesde { get; set; }
        public DateOnly? FechaHasta { get; set; }
    }
}
