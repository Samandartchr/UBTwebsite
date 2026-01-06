using API.Application.Interfaces.Users.ITeacher;
using API.Application.Interfaces.Users.IUser;

namespace API.Application.UseCases.Teachers.InviteStudent;

public record InviteStudentCommand(string token, string groupId, string studentUsername);
public record InviteStudentResult();

public class InviteStudentHandler
{
    private readonly ITeacherWriter _teacherWriter;
    private readonly ITeacherReader _teacherReader;
    private readonly IUserReader _userReader;

    InviteStudentHandler(ITeacherWriter teacherWriter, ITeacherReader teacherReader, IUserReader userReader)
    {
        _teacherWriter = teacherWriter;
        _teacherReader = teacherReader;
        _userReader = userReader;
    }
    public async Task<InviteStudentResult> Handle(InviteStudentCommand cmd)
    {
        //Authorization
        string Id = await _userReader.GetIdAsync(cmd.token);
        if(!await _teacherReader.isTeacher(Id))
        {
            throw new Exception(message: "You are not teacher");
        }

        //Validation

        //Fetch data
        string studentId = await _userReader.GetIdByUsernameAsync(cmd.studentUsername);

        //logic

        //Persist
        await _teacherWriter.InviteStudentToGroupAsync(Id, cmd.groupId, studentId);

        //Side effects
        
        //Return result
        return new InviteStudentResult();
    }
}