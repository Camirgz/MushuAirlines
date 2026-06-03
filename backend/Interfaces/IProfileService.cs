using backend.Model;

namespace backend.Interfaces;

public interface IProfileService
{
    ProfileModel? GetProfileByUsername(string username);

    string UpdateProfile(string username, string role, ProfileUpdateModel profile);
}
