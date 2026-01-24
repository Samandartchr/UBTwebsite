using API.Application.Interfaces.Users.ITeacher;
using API.Application.Interfaces.Users.IUser;
using API.Application.Interfaces.Users.IGroup;

namespace API.Application.UseCases.Teachers.AcceptStudent;

public class AcceptStudentService
{
    private readonly ITeacherWriter _teacherWriter;
    private readonly ITeacherReader _teacherReader;
    private readonly IUserReader _userReader;
    private readonly IGroupReader _groupReader;

    public AcceptStudentService(
        ITeacherWriter teacherWriter,
        ITeacherReader teacherReader,
        IUserReader userReader,
        IGroupReader groupReader)
    {
        _teacherWriter = teacherWriter;
        _teacherReader = teacherReader;
        _userReader = userReader;
        _groupReader = groupReader;
    }

    /// <summary>
    /// Accepts a student into a group. Throws exception if invalid.
    /// </summary>
    public async Task AcceptStudent(string token, string groupId, string studentUsername)
    {
        // Authorization
        if (!await _teacherReader.isTeacher(token))
            throw new Exception("You are not a teacher");

        // Validation
        if (!await _groupReader.isGroupExist(groupId))
            throw new Exception("Group not found");

        // Fetch data
        string studentId = await _userReader.GetIdByUsernameAsync(studentUsername);

        // Logic / Domain

        // Persist
        await _teacherWriter.AcceptStudentToGroupAsync(token, groupId, studentId);

        // Side effects (if any)
    }
}
