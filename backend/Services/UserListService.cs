using backend.Interfaces;
using backend.Model;

namespace backend.Services;

public class UserListService : IUserListService
{
    private readonly IUserListRepository _repository;

    public UserListService(IUserListRepository repository)
    {
        _repository = repository;
    }

    public UserManagementResponseModel GetUsers(int page, int pageSize, string? search)
    {
        var (users, totalCount) = _repository.GetUsers(page, pageSize, search);

        return new UserManagementResponseModel
        {
            Users = users,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public string UpdateUser(int employeeId, UserManagementUpdateModel user)
    {
        if (employeeId <= 0)
        {
            return "El usuario seleccionado no es válido.";
        }

        if (user == null)
        {
            return "Debe ingresar los datos del usuario.";
        }

        NormalizeUser(user);

        string validationMessage = ValidateUser(user);

        if (!string.IsNullOrWhiteSpace(validationMessage))
        {
            return validationMessage;
        }

        try
        {
            bool updated = _repository.UpdateUser(employeeId, user);

            if (!updated)
            {
                return "No se encontró el usuario que desea actualizar.";
            }

            return string.Empty;
        }
        catch (InvalidOperationException ex)
        {
            return ex.Message;
        }
        catch
        {
            return "No se pudo actualizar el usuario.";
        }
    }

    private static void NormalizeUser(UserManagementUpdateModel user)
    {
        user.FirstName = user.FirstName?.Trim() ?? string.Empty;
        user.LastName = user.LastName?.Trim() ?? string.Empty;
        user.Ssn = user.Ssn?.Trim() ?? string.Empty;
        user.Nationality = user.Nationality?.Trim() ?? string.Empty;
        user.WorkSchedule = user.WorkSchedule?.Trim() ?? string.Empty;
        user.Permissions = user.Permissions?.Trim() ?? string.Empty;
        user.Role = user.Role?.Trim() ?? string.Empty;
    }

    private static string ValidateUser(UserManagementUpdateModel user)
    {
        if (string.IsNullOrWhiteSpace(user.FirstName))
        {
            return "Debe ingresar el nombre.";
        }

        if (string.IsNullOrWhiteSpace(user.LastName))
        {
            return "Debe ingresar el apellido.";
        }

        if (string.IsNullOrWhiteSpace(user.Ssn))
        {
            return "Debe ingresar el SSN.";
        }

        if (string.IsNullOrWhiteSpace(user.Nationality))
        {
            return "Debe ingresar la nacionalidad.";
        }

        if (user.Salary == null)
        {
            return "Debe ingresar el salario.";
        }

        if (user.Salary < 0)
        {
            return "El salario no puede ser negativo.";
        }

        if (string.IsNullOrWhiteSpace(user.WorkSchedule))
        {
            return "Debe ingresar el horario.";
        }

        if (string.IsNullOrWhiteSpace(user.Permissions))
        {
            return "Debe ingresar los permisos.";
        }

        if (user.Role != "Administrator" && user.Role != "Operator")
        {
            return "El rol seleccionado no es válido.";
        }

        return string.Empty;
    }
}
