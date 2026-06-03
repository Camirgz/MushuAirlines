using backend.Model;

namespace backend.Interfaces;

public interface IUserListService
{
    UserManagementResponseModel GetUsers(
        int page,
        int pageSize,
        string? search
    );

    string UpdateUser(int employeeId, UserManagementUpdateModel user);
}
