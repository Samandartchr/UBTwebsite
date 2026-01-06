using API.Domain.Entities.User;
using API.Domain.Entities.Test;
using API.Application.Interfaces.Users.IUser;
using API.Application.Interfaces.Users.IGroup;

namespace API.Application.UseCases.Groups.GetGroupResults;

public record GetGroupResultsCommand(string token, string groupId);
public record GetGroupResultsResult(List<TestResultClient> results);

public class GetGroupResultsHandler
{
    private readonly IUserReader _userReader;
    private readonly IGroupReader _groupReader;

    GetGroupResultsHandler(IUserReader userReader, IGroupReader groupReader)
    {
        _userReader = userReader;
        _groupReader = groupReader;
    }
    public async Task<GetGroupResultsResult> Handle(GetGroupResultsCommand cmd)
    {
        //Authorization
        string Id = await _userReader.GetIdAsync(cmd.token);
        if(!await _groupReader.isGroupExist(cmd.groupId))
        {
            throw new Exception(message: "Group does not exist");
        }
        //Validation
        //Fetch data
        List<TestResultClient> results = await _groupReader.GetTestResultsGroupAsync(Id, cmd.groupId);

        //Logic
        //Persists
        //Side effects
        //Return result
        return new GetGroupResultsResult(results: results);
    }
}