using API.Domain.Entities.User;
using API.Application.Interfaces.Users.IGroup;
using API.Application.Interfaces.Users.IUser;

namespace API.Application.UseCases.Groups.GetGroupInfo;

public class GetGroupInfoService
{
    private readonly IGroupReader _groupReader;
    private readonly IUserReader _userReader;

    public GetGroupInfoService(IGroupReader groupReader, IUserReader userReader)
    {
        _groupReader = groupReader;
        _userReader = userReader;
    }

    /// <summary>
    /// Retrieves information about a group if it exists.
    /// Throws if the group does not exist.
    /// </summary>
    public async Task<GroupPublic> GetGroupInfo(string token, string groupId)
    {
        // Authorization
        string userId = await _userReader.GetIdAsync(token);

        // Validation
        if (!await _groupReader.isGroupExist(groupId))
            throw new Exception("Group does not exist");

        // Fetch data
        GroupPublic group = await _groupReader.GetGroupAsync(groupId);

        return group;
    }
}
