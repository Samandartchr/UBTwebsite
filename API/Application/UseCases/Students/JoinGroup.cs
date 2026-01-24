using API.Application.Interfaces.Users.IStudent;
using API.Application.Interfaces.Users.IUser;

namespace API.Application.UseCases.Students.JoinGroup;

public class JoinGroupService
{
    private readonly IStudentWriter _studentWriter;
    private readonly IStudentReader _studentReader;
    private readonly IUserReader _userReader;

    public JoinGroupService(IStudentWriter studentWriter, IStudentReader studentReader, IUserReader userReader)
    {
        _studentWriter = studentWriter;
        _studentReader = studentReader;
        _userReader = userReader;
    }

    /// <summary>
    /// Lets a student join a group. Throws if not a student or already in group.
    /// </summary>
    public async Task JoinGroup(string token, string groupId)
    {
        // Authorization
        string userId = await _userReader.GetIdAsync(token);
        if (!await _studentReader.isStudent(userId))
            throw new Exception("You are not a student");

        // Validation
        if (await _studentReader.IsInGroupAsync(userId, groupId))
            throw new Exception("You are already in group");

        // Persist
        await _studentWriter.JoinGroupAsync(userId, groupId);
    }
}
