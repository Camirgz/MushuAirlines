namespace backend.Exceptions
{
    public class SeatUnavailableException : Exception
    {
        public string SeatNumber { get; }
        public int ScheduledFlightId { get; }

        public SeatUnavailableException(string seatNumber, int scheduledFlightId)
            : base($"El asiento '{seatNumber}' ya está ocupado en el vuelo programado {scheduledFlightId}.")
        {
            SeatNumber = seatNumber;
            ScheduledFlightId = scheduledFlightId;
        }

        public SeatUnavailableException(int scheduledFlightId)
            : base("No hay asientos disponibles en este vuelo.")
        {
            SeatNumber = string.Empty;
            ScheduledFlightId = scheduledFlightId;
        }
    }
}
