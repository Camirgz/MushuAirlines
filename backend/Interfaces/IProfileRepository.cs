using backend.Model;

namespace backend.Interfaces;

public interface IProfileRepository
{
    ProfileModel? GetProfileByUsername(string username);

    bool UpdateBasicProfile(string username, ProfileUpdateModel profile);

    bool UpdateFullProfile(string username, ProfileUpdateModel profile);
}
