namespace backend.Interfaces
{
    public interface IQrService
    {
        byte[] GenerateQr(string text);
    }
}