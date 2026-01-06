using API.Domain.Entities.User;
using API.Application.Interfaces.Users.IUser;
using API.Application.Interfaces.Users.IGroup;

namespace API.Application.UseCases.Groups.GetStudents;

public record GetStudentsCommand(string token, string groupId);
public record GetStudentsResult(List<UserPublicInfo> students);

public class GetStudentsHandler
{
    private readonly IUserReader _userReader;
    private readonly IGroupReader _groupReader;

    GetStudentsHandler(IUserReader userReader, IGroupReader groupReader)
    {
        _userReader = userReader;
        _groupReader = groupReader;
    }
    public async Task<GetStudentsResult> Handle(GetStudentsCommand cmd)
    {
        //Authorization
        string Id = await _userReader.GetIdAsync(cmd.token);
        if(!await _groupReader.isGroupExist(cmd.groupId))
        {
            throw new Exception(message: "Group does not exist");
        }
        //Validation
        //Fetch data
        List<UserPublicInfo> students = await _groupReader.GetGroupStudentsAsync(Id, cmd.groupId);

        //Logic

        //Persists

        //Side effects

        //Return result
        return new GetStudentsResult(students: students);
    }
}