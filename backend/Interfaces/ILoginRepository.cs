namespace backend.Repositories
{
    public interface ILoginRepository
    {
        string GetPasswordHash(string username);

        string GetUserRole(string username);

        string GetRoleByUsername(string username);
    }
}