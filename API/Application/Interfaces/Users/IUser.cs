using API.Domain.Entities.User;
using API.Domain.Entities.Settings;

namespace API.Application.Interfaces.Users.IUser;

public interface IUserReader
{
    bool isUsernameExist(string nickname);
    Task<UserPublicInfo> GetUserPublicInfoAsync(string username);
    Task<string> GetIdAsync(string token); //firebase
    Task<string> GetUsernameAsync(string userId);
    Task<string> GetIdByUsernameAsync(string username);
}

public interface IUserWriter
{
    Task AddUser(User user);
    Task ChangeSettings(string userId, Settings settings);
}