using API.Domain.Enums.Subject;
using API.Domain.Enums.UserRole;
using API.Domain.Entities.Test;
using API.Domain.Entities.User;
using API.Application.Interfaces.Users.IStudent;
using API.Application.Interfaces.Users.IUser;

namespace API.Application.UseCases.Students.GetTest;

public record GetTestCommand(string token, Subject Subject1, Subject Subject2);
public record GetTestResult(Test test);

public class GetTestHandler
{
    private readonly IStudentReader _studentReader;
    private readonly IUserReader _userReader;

    GetTestHandler(IStudentReader studentReader, IUserReader userReader)
    {
        _studentReader = studentReader;
        _userReader = userReader;
    }
    public async Task<GetTestResult> Handle(GetTestCommand cmd)
    {
        //Authorization
        string Id = await _userReader.GetIdAsync(cmd.token);
        if(!await _studentReader.isStudent(Id))
        {
            throw new Exception(message: "You are not student");
        }

        //Validation

        //Fetch data
        Test test = await _studentReader.GetTestAsync(Id, cmd.Subject1, cmd.Subject2);

        //Logic(Domain)

        //Persist

        //Side effects

        //Return result
        return new GetTestResult(test: test);
    }
}