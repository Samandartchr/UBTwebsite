using API.Domain.Entities.Test;
using API.Application.Interfaces.Users.IStudent;
using API.Application.Interfaces.Users.IUser;

namespace API.Application.UseCases.Students.GetTestResults;

public class GetTestResultsService
{
    private readonly IStudentReader _studentReader;
    private readonly IUserReader _userReader;

    public GetTestResultsService(IStudentReader studentReader, IUserReader userReader)
    {
        _studentReader = studentReader;
        _userReader = userReader;
    }

    /// <summary>
    /// Retrieves all test results for a student. Throws if user is not a student.
    /// </summary>
    public async Task<List<TestResultClient>> GetTestResults(string token)
    {
        // Authorization
        string userId = await _userReader.GetIdAsync(token);
        if (!await _studentReader.isStudent(userId))
            throw new Exception("You are not a student");

        // Fetch data
        List<TestResultClient> results = await _studentReader.GetTestResultsAsync(userId);

        // Return results
        return results;
    }
}
