namespace backend.Interfaces;

public interface ICodeGenerator
{
    string GenerateReservationCode();
    string GenerateInvoiceNumber();
}
