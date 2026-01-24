using API.Application.Interfaces.Users.ITeacher;
using API.Application.Interfaces.Users.IUser;

namespace API.Application.UseCases.Teachers.InviteStudent;

public class InviteStudentService
{
    private readonly ITeacherWriter _teacherWriter;
    private readonly ITeacherReader _teacherReader;
    private readonly IUserReader _userReader;

    public InviteStudentService(
        ITeacherWriter teacherWriter,
        ITeacherReader teacherReader,
        IUserReader userReader)
    {
        _teacherWriter = teacherWriter;
        _teacherReader = teacherReader;
        _userReader = userReader;
    }

    /// <summary>
    /// Invites a student to a group. Throws exception if not a teacher.
    /// </summary>
    public async Task InviteStudent(string token, string groupId, string studentUsername)
    {
        // Authorization
        string userId = await _userReader.GetIdAsync(token);
        if (!await _teacherReader.isTeacher(userId))
            throw new Exception("You are not a teacher");

        // Fetch data
        string studentId = await _userReader.GetIdByUsernameAsync(studentUsername);

        // Logic / Domain

        // Persist
        await _teacherWriter.InviteStudentToGroupAsync(userId, groupId, studentId);

        // Side effects (if any)
    }
}
