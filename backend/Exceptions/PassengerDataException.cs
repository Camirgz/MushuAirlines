namespace backend.Exceptions
{
    public class PassengerDataException : Exception
    {
        public string Field { get; }

        public PassengerDataException(string field, string reason)
            : base($"Dato inválido en el campo '{field}': {reason}")
        {
            Field = field;
        }
    }
}
