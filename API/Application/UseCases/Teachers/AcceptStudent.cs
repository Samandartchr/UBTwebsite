using API.Application.Interfaces.Users.ITeacher;
using API.Application.Interfaces.Users.IUser;

namespace API.Application.UseCases.Teachers.AcceptStudent;

public record AcceptStudentCommand(string token, string groupId, string studentUsername);
public record AcceptStudentResult();

public class AcceptStudentHandler
{
    private readonly ITeacherWriter _teacherWriter;
    private readonly ITeacherReader _teacherReader;
    private readonly IUserReader _userReader;
    AcceptStudentHandler(ITeacherWriter teacherWriter, ITeacherReader teacherReader, IUserReader userReader)
    {
        _teacherWriter = teacherWriter;
        _teacherReader = teacherReader;
        _userReader = userReader;
    }
    public async Task<AcceptStudentResult> Handle(AcceptStudentCommand cmd)
    {
        //Authorization
        if(!await _teacherReader.isTeacher(cmd.token))
        {
            throw new Exception(message: "You are not teacher");
        }

        //Validation
        if(!await _teacherReader.isGroupExist(cmd.groupId))
        {
            throw new Exception(message: "Group not found");
        }
        //Fetch data
        string studentId = await _userReader.GetIdByUsernameAsync(cmd.studentUsername);

        //Logic

        //Persist
        await _teacherWriter.AcceptStudentToGroupAsync(cmd.token, cmd.groupId, studentId);

        //Side effects

        //Return result
        return new AcceptStudentResult();
    }
}