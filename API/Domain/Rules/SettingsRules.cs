using API.Domain.Entities.Settings;
using API.Domain.Rules.RegisterRules;

namespace API.Domain.Rules.SettingsRules;

public static class SettingsValidator
{
    public static bool isSettingsValid(Settings settings)
    {
        RegisterRules.RegisterValidator.isNameOrSurnameValid(settings.Name);
        RegisterRules.RegisterValidator.isNameOrSurnameValid(settings.Surname);

        if (string.IsNullOrWhiteSpace(settings.ProfileImageLink))
        {
            settings.ProfileImageLink = "";
        }

        if(settings.PhoneNumber == null)
        {
            settings.PhoneNumber = "";
            return true;
        }
        if (!isPhoneNumberValid(settings.PhoneNumber))
        {
            return false;
        }
        
        return true;
    }

    public static bool isPhoneNumberValid(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber)){return false;}
        foreach (char c in phoneNumber)
        {
            if (!char.IsDigit(c) || c != '+'){return false;}
        }
        return true;
    }
}