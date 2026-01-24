using API.Application.Interfaces.Users.IStudent;
using API.Application.Interfaces.Users.IUser;

namespace API.Application.UseCases.Students.AcceptInvite;

public class AcceptInviteService
{
    private readonly IStudentWriter _studentWriter;
    private readonly IStudentReader _studentReader;
    private readonly IUserReader _userReader;

    public AcceptInviteService(
        IStudentWriter studentWriter,
        IStudentReader studentReader,
        IUserReader userReader)
    {
        _studentWriter = studentWriter;
        _studentReader = studentReader;
        _userReader = userReader;
    }

    /// <summary>
    /// Accepts a group invite for a student. Throws exception if invalid.
    /// </summary>
    public async Task AcceptInvite(string token, string groupId)
    {
        // Authorization
        string userId = await _userReader.GetIdAsync(token);
        if (!await _studentReader.isStudent(userId))
            throw new Exception("You are not a student");

        // Validation
        if (await _studentReader.IsInGroupAsync(userId, groupId))
            throw new Exception("You are already in the group");

        // Logic / Domain

        // Persist
        await _studentWriter.AcceptGroupInviteAsync(userId, groupId);

        // Side effects (if any)
    }
}
