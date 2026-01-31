using API.Domain.Entities.User;
using API.Application.Interfaces.Users.IUser;
using API.Domain.Rules.RegisterRules;

namespace API.Application.UseCases.Users.Register;

public class RegisterService
{
    private readonly IUserReader _userReader;
    private readonly IUserWriter _userWriter;

    public RegisterService(IUserReader userReader, IUserWriter userWriter)
    {
        _userReader = userReader;
        _userWriter = userWriter;
    }

    /// <summary>
    /// Executes user registration.
    /// Throws Exception if validation fails.
    /// </summary>
    public async Task<bool> Register(UserRegister userRegister, string firebaseToken)
    {
        // Authorization
        string userId = await _userReader.GetIdAsync(firebaseToken);

        // Validation
        if (!RegisterValidator.isRegistrationValid(userRegister))
            throw new Exception("Invalid registration");

        if (_userReader.isUsernameExist(userRegister.Username))
            throw new Exception("Username already exists");

        if (string.IsNullOrWhiteSpace(firebaseToken))
            throw new Exception("Invalid Firebase token");

        // Logic / Domain
        User user = new User
        {
            Username = userRegister.Username,
            Email = userRegister.Email,
            Role = userRegister.Role,
            Name = userRegister.Name,
            Surname = userRegister.Surname,
            CreatedAt = DateTime.UtcNow,
            isPremium = false,
            Id = userId
        };

        // Persist
        await _userWriter.AddUser(user);

        // Side effects (if any)

        return true;
    }
}
