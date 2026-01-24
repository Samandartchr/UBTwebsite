using API.Domain.Enums.Subject;
using API.Domain.Entities.Test;
using API.Application.Interfaces.Users.IStudent;
using API.Application.Interfaces.Users.IUser;

namespace API.Application.UseCases.Students.GetTest;

public class GetTestService
{
    private readonly IStudentReader _studentReader;
    private readonly IUserReader _userReader;

    public GetTestService(IStudentReader studentReader, IUserReader userReader)
    {
        _studentReader = studentReader;
        _userReader = userReader;
    }

    /// <summary>
    /// Returns a test for the student for the given subjects. Throws if not a student.
    /// </summary>
    public async Task<Test> GetTest(string token, Subject subject1, Subject subject2)
    {
        // Authorization
        string userId = await _userReader.GetIdAsync(token);
        if (!await _studentReader.isStudent(userId))
            throw new Exception("You are not a student");

        // Fetch test data
        Test test = await _studentReader.GetTestAsync(userId, subject1, subject2);

        // Return result
        return test;
    }
}
