using API.Domain.Entities.User;
using API.Application.Interfaces.Users.IUser;
using API.Application.Interfaces.Users.IStudent;

namespace API.Application.UseCases.Students.GetGroups;

public record GetGroupsCommand(string token);
public record GetGroupsResult(List<GroupPublic> groups);

public class GetGroupsHandler
{
    private readonly IStudentReader _studentReader;
    private readonly IUserReader _userReader;

    GetGroupsHandler(IStudentReader studentReader, IUserReader userReader)
    {
        _studentReader = studentReader;
        _userReader = userReader;
    }
    public async Task<GetGroupsResult> Handle(GetGroupsCommand cmd)
    {
        //Authorization
        string Id = await _userReader.GetIdAsync(cmd.token);
        if(!await _studentReader.isStudent(Id))
        {
            throw new Exception(message: "You are not student");
        }
        //Validation

        //Fetch data
        List<GroupPublic> groups = await _studentReader.GetGroups(Id);

        //Logic
        
        //Persist

        //Side effects

        //Return result
        return new GetGroupsResult(groups: groups);
    }
}