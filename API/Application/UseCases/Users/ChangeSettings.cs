using API.Domain.Entities.Settings;
using API.Application.Interfaces.Users.IUser;
using API.Domain.Rules.SettingsRules;


namespace API.Application.UseCases.Users.ChangeSettings;

public record ChangeSettingsCommand(string token, Settings settings);
public record ChangeSettingsResult(bool isChanged);

public class ChangeSettingsHandler
{
    private readonly IUserWriter _userWriter;
    private readonly IUserReader _userReader;

    ChangeSettingsHandler(IUserWriter userWriter, IUserReader userReader)
    {
        _userWriter = userWriter;
        _userReader = userReader;
    }

    public async Task<ChangeSettingsResult> Handle(ChangeSettingsCommand cmd)
    {
        //Authorization
        string userId = await _userReader.GetIdAsync(cmd.token);
        
        //Validation
        if (!SettingsValidator.isSettingsValid(cmd.settings))
        {
            throw new Exception(message: "Invalid settings");
        }

        //Fetch data

        //Logic(Domain)

        //Persist
        await _userWriter.ChangeSettings(userId, cmd.settings);

        //Side effects
        
        //Return result
        return new ChangeSettingsResult(isChanged: true);
    }
}