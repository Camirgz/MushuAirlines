using backend.Model;

namespace backend.Interfaces;

public interface IUserListRepository
{
    (List<UserManagementItemModel> Users, int TotalCount) GetUsers(
        int page,
        int pageSize,
        string? search
    );

    bool UpdateUser(int employeeId, UserManagementUpdateModel user);

    bool DeleteUser(int employeeId);
}
