using API.Domain.Entities.Settings;
using API.Domain.Rules.RegisterRules;

namespace API.Domain.Rules.SettingsRules;

public static class SettingsValidator
{
    public static bool isSettingsValid(Settings settings)
    {
        if (!RegisterValidator.isNameOrSurnameValid(settings.Name)) return false;
        if (!RegisterValidator.isNameOrSurnameValid(settings.Surname)) return false;

        if (string.IsNullOrWhiteSpace(settings.ProfileImageLink))
            settings.ProfileImageLink = "";

        if (settings.PhoneNumber == null)
        {
            settings.PhoneNumber = "";
            return true;
        }

        return isPhoneNumberValid(settings.PhoneNumber);
    }

    public static bool isPhoneNumberValid(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber)) return false;

        foreach (char c in phoneNumber)
        {
            if (!char.IsDigit(c) && c != '+') return false;
        }
        return true;
    }
}