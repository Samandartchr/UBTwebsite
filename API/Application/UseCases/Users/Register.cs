using API.Domain.Entities.User;
using API.Application.Interfaces.Users.IUser;
using API.Domain.Enums.UserRole;
using API.Domain.Rules.RegisterRules;

namespace API.Application.UseCases.Users.Register;

public record RegisterCommand(UserRegister userRegister, string FirebaseToken);
public record RegisterResult(bool isSucceed);

public class RegisterHandler
{
    private readonly IUserReader _userReader;
    private readonly IUserWriter _userWriter;

    RegisterHandler(IUserReader userReader, IUserWriter userWriter)
    {
        _userReader = userReader;
        _userWriter = userWriter;
    }

    public async Task<RegisterResult> Handle(RegisterCommand cmd)
    {
        //Authorization, can user do this?
        string userId = await _userReader.GetIdAsync(cmd.FirebaseToken);

        //Validation
        if (!RegisterValidator.isRegistrationValid(cmd.userRegister))
        {
            throw new Exception(message: "Invalid registration");
        }
        else if (await _userReader.isUsernameExist(cmd.userRegister.Username))
        {
            throw new Exception(message: "Username already exist");
        }
        else if (string.IsNullOrWhiteSpace(cmd.FirebaseToken)){throw new Exception(message: "Invalid Firebase token");}

        //Fetch data

        //Logic(Domain)
        User user = new User
        {
            Username = cmd.userRegister.Username,
            Email = cmd.userRegister.Email,
            Role = cmd.userRegister.Role,
            Name = cmd.userRegister.Name,
            Surname = cmd.userRegister.Surname,
            CreatedAt = DateTime.UtcNow,
            isPremium = false,
            Id = userId
        };
        
        //Persist
        await _userWriter.AddUser(user);

        //Side effects

        //Return result
        return new RegisterResult(isSucceed: true);
    }
}