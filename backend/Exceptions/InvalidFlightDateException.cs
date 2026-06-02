namespace backend.Exceptions
{
    public class InvalidFlightDateException : Exception
    {
        public DateOnly RequestedDate { get; }
        public string RouteCode { get; }

        public InvalidFlightDateException(DateOnly requestedDate, string routeCode, string reason)
            : base($"La fecha {requestedDate:yyyy-MM-dd} no es válida para la ruta '{routeCode}': {reason}")
        {
            RequestedDate = requestedDate;
            RouteCode = routeCode;
        }
    }
}
