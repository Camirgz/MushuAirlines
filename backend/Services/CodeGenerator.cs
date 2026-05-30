using System.Security.Cryptography;
using backend.Interfaces;

namespace backend.Services;

public class CodeGenerator : ICodeGenerator
{
    private const string Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
    private const int ReservationCodeLength = 6;

    public string GenerateReservationCode()
    {
        return GenerateRandomString(ReservationCodeLength);
    }

    public string GenerateInvoiceNumber()
    {
        string datePart   = DateTime.UtcNow.ToString("yyyyMMdd");
        string randomPart = GenerateRandomString(8);
        return $"MA-{datePart}-{randomPart}";
    }

    private static string GenerateRandomString(int length)
    {
        byte[] randomBytes = RandomNumberGenerator.GetBytes(length);

        char[] chars = new char[length];
        for (int i = 0; i < length; i++)
        {
            chars[i] = Alphabet[randomBytes[i] % Alphabet.Length];
        }

        return new string(chars);
    }
}
