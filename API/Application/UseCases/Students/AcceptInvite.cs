using API.Application.Interfaces.Users.IStudent;
using API.Application.Interfaces.Users.IUser;

namespace API.Application.UseCases.Students.AcceptInvite;

public record AcceptInviteCommand(string token, string groupId);
public record AcceptInviteResult();

public class AcceptInviteHandler
{
    private readonly IStudentWriter _studentWriter;
    private readonly IStudentReader _studentReader;
    private readonly IUserReader _userReader;
    AcceptInviteHandler(IStudentWriter studentWriter, IStudentReader studentReader, IUserReader userReader)
    {
        _studentWriter = studentWriter;
        _studentReader = studentReader;
        _userReader = userReader;
    }

    public async Task<AcceptInviteResult> Handle(AcceptInviteCommand cmd)
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
        await _studentWriter.AcceptGroupInviteAsync(Id, cmd.groupId);

        //Side effects

        //Return result
        return new AcceptInviteResult();
    }
}