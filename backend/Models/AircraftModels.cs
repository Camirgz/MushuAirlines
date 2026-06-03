namespace backend.Model
{
    public class AircraftResponseModel
    {
        public int Id { get; set; }
        public string Model { get; set; }
        public string Type { get; set; }
        public double WeightKg { get; set; }

        public int EconomyRows { get; set; }
        public int EconomySeatsPerRow { get; set; }

        public int FirstClassRows { get; set; }
        public int FirstClassSeatsPerRow { get; set; }

        public int Capacity { get; set; }
    }

    public class CreateAircraftRequestModel
    {
        public string Model { get; set; }
        public string Type { get; set; }
        public double WeightKg { get; set; }

        public int EconomyRows { get; set; }
        public int EconomySeatsPerRow { get; set; }

        public int? FirstClassRows { get; set; }
        public int? FirstClassSeatsPerRow { get; set; }
    }

    public class UpdateAircraftRequestModel
    {
        public double WeightKg { get; set; }

        public int EconomyRows { get; set; }
        public int EconomySeatsPerRow { get; set; }

        public int FirstClassRows { get; set; }
        public int FirstClassSeatsPerRow { get; set; }
    }
}
