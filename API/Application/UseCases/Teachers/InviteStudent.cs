using API.Application.Interfaces.Users.ITeacher;
using API.Application.Interfaces.Users.IUser;
using API.Application.Interfaces.Users.IStudent;

namespace API.Application.UseCases.Teachers.InviteStudent;

public class InviteStudentService
{
    private readonly ITeacherWriter _teacherWriter;
    private readonly ITeacherReader _teacherReader;
    private readonly IUserReader _userReader;
    private readonly IStudentReader _studentReader;

    public InviteStudentService(
        ITeacherWriter teacherWriter,
        ITeacherReader teacherReader,
        IUserReader userReader,
        IStudentReader studentReader)
    {
        _teacherWriter = teacherWriter;
        _teacherReader = teacherReader;
        _userReader = userReader;
        _studentReader = studentReader;
    }

    /// <summary>
    /// Invites a student to a group. Throws exception if not a teacher.
    /// </summary>
    public async Task InviteStudent(string token, string groupId, string studentUsername)
    {
        bool isTeacher = true;
        bool isStudent = true;
        // Authorization
        string userId = await _userReader.GetIdAsync(token);
        if (!await _teacherReader.isTeacher(userId))
        {
            isTeacher = false;
            throw new Exception("You are not a teacher");
        }


        // Fetch data
        string studentId = await _userReader.GetIdByUsernameAsync(studentUsername);
        if (!await _studentReader.isStudent(studentId))
        {
            isStudent = false;
            throw new Exception("The specified user is not a student");
        }

        // Logic / Domain
        if (await _studentReader.IsInGroupAsync(studentId, groupId)) 
            throw new Exception("The student is already in the group");

        // Persist
        if (isTeacher && isStudent)
            await _teacherWriter.InviteStudentToGroupAsync(userId, groupId, studentId);
        else throw new Exception("Invitation failed due to invalid teacher or student status");

        // Side effects (if any)
    }
}
