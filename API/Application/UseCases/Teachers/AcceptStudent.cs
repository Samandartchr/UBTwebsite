using API.Application.Interfaces.Users.ITeacher;
using API.Application.Interfaces.Users.IUser;
using API.Application.Interfaces.Users.IGroup;
using API.Application.Interfaces.Users.IStudent;

namespace API.Application.UseCases.Teachers.AcceptStudent;

public class AcceptStudentService
{
    private readonly ITeacherWriter _teacherWriter;
    private readonly ITeacherReader _teacherReader;
    private readonly IUserReader _userReader;
    private readonly IGroupReader _groupReader;
    private readonly IStudentReader _studentReader;

    public AcceptStudentService(
        ITeacherWriter teacherWriter,
        ITeacherReader teacherReader,
        IUserReader userReader,
        IGroupReader groupReader,
        IStudentReader studentReader)
    {
        _teacherWriter = teacherWriter;
        _teacherReader = teacherReader;
        _userReader = userReader;
        _groupReader = groupReader;
        _studentReader = studentReader;
    }

    /// <summary>
    /// Accepts a student into a group. Throws exception if invalid.
    /// </summary>
    public async Task AcceptStudent(string token, string groupId, string studentUsername)
    {
        // Authorization
        string userId = await _userReader.GetIdAsync(token);
        if (!await _teacherReader.isTeacher(userId))
            throw new Exception("You are not a teacher");

        // Validation
        if (!await _groupReader.isGroupExist(groupId))
            throw new Exception("Group not found");

        // Fetch data
        string studentId = await _userReader.GetIdByUsernameAsync(studentUsername);
        if(await _studentReader.IsInGroupAsync(studentId, groupId))
        {
            throw new Exception("Student is already in the group");
        }
        // Logic / Domain

        // Persist
        await _teacherWriter.AcceptStudentToGroupAsync(token, groupId, studentId);

        // Side effects (if any)
    }
}
