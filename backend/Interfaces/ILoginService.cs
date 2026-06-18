using backend.Model;

namespace backend.Services
{
    public interface ILoginService
    {
        string Login(LoginModel login);
    }
}