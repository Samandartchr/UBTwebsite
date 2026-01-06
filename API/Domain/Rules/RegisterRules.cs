using API.Domain.Entities.User;
using API.Domain.Enums.UserRole;

namespace API.Domain.Rules.RegisterRules;

public static class RegisterValidator
{
    public static bool isUsernameValid(string username)
    {
        if (username.Length < 4 || username.Length > 15 || string.IsNullOrWhiteSpace(username)){return false;}
        foreach (char c in username)
        {
            if (!char.IsLetterOrDigit(c) && c != '.' && c != '_')
            {
                return false;
            }
        }
        return true;

    }
    public static bool isEmailValid(string email)
    {
        if (string.IsNullOrWhiteSpace(email)){return false;}
        else if (!email.Contains('@') || !email.Contains('.')){return false;}
        return true;
    }
    public static bool isNameOrSurnameValid(string name)
    {
        if (string.IsNullOrWhiteSpace(name)){return false;}
        foreach (char c in name)
        {
            if (!char.IsLetter(c)){return false;}
        }
        return true;
    }

    public static bool isRegistrationValid(UserRegister userRegister)
    {
        if (!isUsernameValid(userRegister.Username)){return false;}
        else if (!isEmailValid(userRegister.Email)){return false;}
        else if (!isNameOrSurnameValid(userRegister.Name)){return false;}
        else if (!isNameOrSurnameValid(userRegister.Surname)){return false;}
        return true;
    }
}