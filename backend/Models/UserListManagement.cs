namespace backend.Model;

public class UserManagementItemModel
{
    public int Id { get; set; }

    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;

    public string Ssn { get; set; } = string.Empty;
    public string Nationality { get; set; } = string.Empty;

    // Se muestra, pero no se modifica.
    public string Email { get; set; } = string.Empty;

    public double Salary { get; set; }
    public string WorkSchedule { get; set; } = string.Empty;
    public string Permissions { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;
}

public class UserManagementUpdateModel
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Ssn { get; set; }
    public string? Nationality { get; set; }

    public double? Salary { get; set; }
    public string? WorkSchedule { get; set; }
    public string? Permissions { get; set; }
    public string? Role { get; set; }

}

public class UserManagementResponseModel
{
    public List<UserManagementItemModel> Users { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}