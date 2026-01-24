using API.Domain.Entities.User;
using API.Application.Interfaces.Users.IUser;
using API.Application.Interfaces.Users.IGroup;

namespace API.Application.UseCases.Groups.GetStudents;

public class GetStudentsService
{
    private readonly IUserReader _userReader;
    private readonly IGroupReader _groupReader;

    public GetStudentsService(IUserReader userReader, IGroupReader groupReader)
    {
        _userReader = userReader;
        _groupReader = groupReader;
    }

    /// <summary>
    /// Retrieves all students in a group. Throws if the group does not exist.
    /// </summary>
    public async Task<List<UserPublicInfo>> GetStudents(string token, string groupId)
    {
        // Authorization
        string userId = await _userReader.GetIdAsync(token);

        // Validation
        if (!await _groupReader.isGroupExist(groupId))
            throw new Exception("Group does not exist");

        // Fetch data
        List<UserPublicInfo> students = await _groupReader.GetGroupStudentsAsync(userId, groupId);

        return students;
    }
}
