using API.Application.Interfaces.Users.ITeacher;
using API.Application.Interfaces.Users.IUser;

namespace API.Application.UseCases.Teachers.RemoveStudent;

public record RemoveStudentCommand(string token, string groupId, string studentUsername);
public record RemoveStudentResult();

public class RemoveStudentHandler
{
    private readonly ITeacherWriter _teacherWriter;
    private readonly ITeacherReader _teacherReader;
    private readonly IUserReader _userReader;
    RemoveStudentHandler(ITeacherWriter teacherWriter, ITeacherReader teacherReader, IUserReader userReader)
    {
        _teacherWriter = teacherWriter;
        _teacherReader = teacherReader;
        _userReader = userReader;
    }

    public async Task<RemoveStudentResult> Handle(RemoveStudentCommand cmd)
    {
        //Authorization
        string Id = await _userReader.GetIdAsync(cmd.token);
        if(!await _teacherReader.isTeacher(Id))
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
        await _teacherWriter.RemoveStudentFromGroupAsync(Id, cmd.groupId, studentId);
        
        //Side effects

        //Return result
        return new RemoveStudentResult();
    }
}