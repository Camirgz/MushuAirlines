using backend.Interfaces;
using backend.Model;

namespace backend.Services;

public class ProfileService : IProfileService
{
    private readonly IProfileRepository _profileRepository;

    public ProfileService(IProfileRepository profileRepository)
    {
        _profileRepository = profileRepository;
    }

    public ProfileModel? GetProfileByUsername(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            return null;
        }

        return _profileRepository.GetProfileByUsername(username.Trim());
    }

    public string UpdateProfile(string username, string role, ProfileUpdateModel profile)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            return "No se pudo identificar el usuario conectado.";
        }

        if (string.IsNullOrWhiteSpace(role))
        {
            return "No se pudo identificar el rol del usuario conectado.";
        }

        if (profile == null)
        {
            return "Debe ingresar los datos del perfil.";
        }

        NormalizeProfile(profile);

        ProfileModel? currentProfile = _profileRepository.GetProfileByUsername(username.Trim());

        if (currentProfile == null)
        {
            return "No existe un perfil asociado al usuario conectado.";
        }

        string validationMessage = role == "Administrator"
            ? ValidateAdminProfile(profile)
            : ValidateOperatorProfile(profile);

        if (!string.IsNullOrWhiteSpace(validationMessage))
        {
            return validationMessage;
        }

        if (role == "Administrator" && IsSameAdminProfile(currentProfile, profile))
        {
            return "No se realizó ningún cambio porque los datos son iguales.";
        }

        if (role == "Operator" && IsSameOperatorProfile(currentProfile, profile))
        {
            return "No se realizó ningún cambio porque los datos son iguales.";
        }

        try
        {
            bool updated = role == "Administrator"
                ? _profileRepository.UpdateFullProfile(username.Trim(), profile)
                : _profileRepository.UpdateBasicProfile(username.Trim(), profile);

            if (!updated)
            {
                return "No se pudo actualizar el perfil.";
            }

            return string.Empty;
        }
        catch (InvalidOperationException ex)
        {
            return ex.Message;
        }
        catch
        {
            return "No se pudo actualizar el perfil.";
        }
    }

    private static void NormalizeProfile(ProfileUpdateModel profile)
    {
        profile.FirstName = profile.FirstName?.Trim() ?? string.Empty;
        profile.LastName = profile.LastName?.Trim() ?? string.Empty;
        profile.Ssn = profile.Ssn?.Trim() ?? string.Empty;
        profile.Nationality = profile.Nationality?.Trim() ?? string.Empty;
        profile.WorkSchedule = profile.WorkSchedule?.Trim() ?? string.Empty;
        profile.Permissions = profile.Permissions?.Trim() ?? string.Empty;
        profile.Role = profile.Role?.Trim() ?? string.Empty;
    }

    private static string ValidateOperatorProfile(ProfileUpdateModel profile)
    {
        if (string.IsNullOrWhiteSpace(profile.FirstName))
        {
            return "Debe ingresar el nombre.";
        }

        if (string.IsNullOrWhiteSpace(profile.LastName))
        {
            return "Debe ingresar el apellido.";
        }

        if (string.IsNullOrWhiteSpace(profile.Ssn))
        {
            return "Debe ingresar el SSN.";
        }

        if (string.IsNullOrWhiteSpace(profile.Nationality))
        {
            return "Debe ingresar la nacionalidad.";
        }

        return string.Empty;
    }

    private static string ValidateAdminProfile(ProfileUpdateModel profile)
    {
        string basicValidation = ValidateOperatorProfile(profile);

        if (!string.IsNullOrWhiteSpace(basicValidation))
        {
            return basicValidation;
        }

        if (profile.Salary == null)
        {
            return "Debe ingresar el salario.";
        }

        if (profile.Salary < 0)
        {
            return "El salario no puede ser negativo.";
        }

        if (string.IsNullOrWhiteSpace(profile.WorkSchedule))
        {
            return "Debe ingresar el horario.";
        }

        if (string.IsNullOrWhiteSpace(profile.Permissions))
        {
            return "Debe ingresar los permisos.";
        }

        if (profile.Role != "Administrator" && profile.Role != "Operator")
        {
            return "El rol seleccionado no es válido.";
        }

        return string.Empty;
    }

    private static bool IsSameOperatorProfile(ProfileModel current, ProfileUpdateModel updated)
    {
        return
            string.Equals(current.FirstName, updated.FirstName, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(current.LastName, updated.LastName, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(current.Ssn, updated.Ssn, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(current.Nationality, updated.Nationality, StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsSameAdminProfile(ProfileModel current, ProfileUpdateModel updated)
    {
        return
            IsSameOperatorProfile(current, updated) &&
            current.Salary == updated.Salary &&
            string.Equals(current.WorkSchedule, updated.WorkSchedule, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(current.Permissions, updated.Permissions, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(current.Role, updated.Role, StringComparison.OrdinalIgnoreCase);
    }
}
