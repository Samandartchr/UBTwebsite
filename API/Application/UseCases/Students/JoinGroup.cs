using API.Application.Interfaces.Users.IStudent;
using API.Application.Interfaces.Users.IUser;

namespace API.Application.UseCases.Students.JoinGroup;

public record JoinGroupCommand(string token, string groupId);
public record JoinGroupResult();

public class JoinGroupHandler
{
    private readonly IStudentWriter _studentWriter;
    private readonly IStudentReader _studentReader;
    private readonly IUserReader _userReader;
    JoinGroupHandler(IStudentWriter studentWriter, IStudentReader studentReader, IUserReader userReader)
    {
        _studentWriter = studentWriter;
        _studentReader = studentReader;
        _userReader = userReader;
    }
    public async Task<JoinGroupResult> Handle(JoinGroupCommand cmd)
    {
        //Authorization
        string Id = await _userReader.GetIdAsync(cmd.token);
        if(!await _studentReader.isStudent(Id))
        {
            throw new Exception(message: "You are not student");
        }
        //Validation
        if(await _studentReader.IsStudentInGroupAsync(Id, cmd.groupId))
        {
            throw new Exception(message: "You are already in group");
        }
        //Fetch data

        //Logic
        
        //Persist
        await _studentWriter.JoinGroupAsync(Id, cmd.groupId);

        //Side effects

        //Return result
        return new JoinGroupResult();
    }
}