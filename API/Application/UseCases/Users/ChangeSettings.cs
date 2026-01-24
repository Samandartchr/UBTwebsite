using API.Domain.Entities.Settings;
using API.Application.Interfaces.Users.IUser;
using API.Domain.Rules.SettingsRules;

namespace API.Application.UseCases.Users.ChangeSettings;

public class ChangeSettingsService
{
    private readonly IUserWriter _userWriter;
    private readonly IUserReader _userReader;

    public ChangeSettingsService(IUserWriter userWriter, IUserReader userReader)
    {
        _userWriter = userWriter;
        _userReader = userReader;
    }

    /// <summary>
    /// Changes user settings. Throws exception if invalid.
    /// </summary>
    public async Task<bool> ChangeSettings(string token, Settings settings)
    {
        // Authorization
        string userId = await _userReader.GetIdAsync(token);

        // Validation
        if (!SettingsValidator.isSettingsValid(settings))
            throw new Exception("Invalid settings");

        // Logic / Domain

        // Persist
        await _userWriter.ChangeSettings(userId, settings);

        // Side effects (if any)

        return true;
    }
}
