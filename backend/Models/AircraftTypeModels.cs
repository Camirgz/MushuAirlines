namespace backend.Model
{
    public class AircraftTypeOptionModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? DefaultModel { get; set; }
        public double? DefaultWeightKg { get; set; }
        public int? DefaultEconomyRows { get; set; }
        public int? DefaultEconomySeatsPerRow { get; set; }
        public int? DefaultFirstClassRows { get; set; }
        public int? DefaultFirstClassSeatsPerRow { get; set; }
    }
}
