using API.Domain.Entities.Test;
using API.Application.Interfaces.Users.IStudent;
using API.Application.Interfaces.Users.IUser;

namespace API.Application.UseCases.Students.GetTestResults;

public record GetTestResultsCommand(string token);
public record GetTestResultsResult(List<TestResult> results);

public class GetTestResultsHandler
{
    private readonly IStudentReader _studentReader;
    private readonly IUserReader _userReader;

    GetTestResultsHandler(IStudentReader studentReader, IUserReader userReader)
    {
        _studentReader = studentReader;
        _userReader = userReader;
    }
    public async Task<GetTestResultsResult> Handle(GetTestResultsCommand cmd)
    {
        //Authorization
        string Id = await _userReader.GetIdAsync(cmd.token);
        if(!await _studentReader.isStudent(Id))
        {
            throw new Exception(message: "You are not student");
        }

        //Validation

        //Fetch data
        List<TestResult> results = await _studentReader.GetTestResultsAsync(Id);

        //Logic(Domain)

        //Persist

        //Side effects

        //Return result
        return new GetTestResultsResult(results: results);
    }
}