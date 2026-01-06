using API.Domain.Entities.User;
using API.Application.Interfaces.Users.IGroup;
using API.Application.Interfaces.Users.IUser;

namespace API.Application.UseCases.Groups.GetGroupInfo;

public record GetGroupInfoCommand(string token, string groupId);
public record GetGroupInfoResult(GroupPublic group);
public class GetGroupInfoHandler
{
    private readonly IGroupReader _groupReader;
    private readonly IUserReader _userReader;

    GetGroupInfoHandler(IGroupReader groupReader, IUserReader userReader)
    {
        _groupReader = groupReader;
        _userReader = userReader;
    }
    
    public async Task<GetGroupInfoResult> Handle(GetGroupInfoCommand cmd)
    {
        //Authorization
        string Id = await _userReader.GetIdAsync(cmd.token);
        if(!await _groupReader.isGroupExist(cmd.groupId))
        {
            throw new Exception(message: "Group does not exist");
        }
        //Validation

        //Fetch data
        GroupPublic group = await _groupReader.GetGroupAsync(cmd.groupId);

        //Logic

        //Persists

        //Side effects

        //Return result
        return new GetGroupInfoResult(group: group);
    }
}