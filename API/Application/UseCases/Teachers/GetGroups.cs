using API.Domain.Entities.User;
using API.Application.Interfaces.Users.IUser;
using API.Application.Interfaces.Users.ITeacher;

namespace API.Application.UseCases.Teachers.GetGroups;

public record GetGroupsCommand(string token);
public record GetGroupsResult(List<GroupPublic> groups);

public class GetGroupsHandler
{
    private readonly ITeacherReader _teacherReader;
    private readonly IUserReader _userReader;
    GetGroupsHandler(ITeacherReader teacherReader, IUserReader userReader)
    {
        _teacherReader = teacherReader;
        _userReader = userReader;
    }
    public async Task<GetGroupsResult> Handle(GetGroupsCommand cmd)
    {
        //Authorization
        string Id = await _userReader.GetIdAsync(cmd.token);
        if(!await _teacherReader.isTeacher(Id))
        {
            throw new Exception(message: "You are not teacher");
        }

        //Validation

        //Fetch data
        List<GroupPublic> groups = await _teacherReader.GetTeacherGroupsAsync(Id);

        //Logic(Domain)
        
        //Persist

        //Side effects

        //Return result
        return new GetGroupsResult(groups: groups);
    }
}