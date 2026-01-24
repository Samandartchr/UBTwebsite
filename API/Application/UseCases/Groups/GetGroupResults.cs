using API.Domain.Entities.User;
using API.Domain.Entities.Test;
using API.Application.Interfaces.Users.IUser;
using API.Application.Interfaces.Users.IGroup;

namespace API.Application.UseCases.Groups.GetGroupResults;

public class GetGroupResultsService
{
    private readonly IUserReader _userReader;
    private readonly IGroupReader _groupReader;

    public GetGroupResultsService(IUserReader userReader, IGroupReader groupReader)
    {
        _userReader = userReader;
        _groupReader = groupReader;
    }

    /// <summary>
    /// Retrieves all test results for a group.
    /// Throws if the group does not exist.
    /// </summary>
    public async Task<List<TestResultClient>> GetGroupResults(string token, string groupId)
    {
        // Authorization
        string userId = await _userReader.GetIdAsync(token);

        // Validation
        if (!await _groupReader.isGroupExist(groupId))
            throw new Exception("Group does not exist");

        // Fetch data
        List<TestResultClient> results = await _groupReader.GetTestResultsGroupAsync(userId, groupId);

        return results;
    }
}
