using API.Application.Interfaces.Users.IStudent;
using API.Application.Interfaces.Users.IUser;
using API.Domain.Entities.Test;

namespace API.Application.UseCases.Students.PassTest;

public record PassTestCommand(string token, Test Test);
public record PassTestResult(TestResultClient result);

public class PassTestHandler
{
    private readonly IStudentWriter _studentWriter;
    private readonly IStudentReader _studentReader;
    private readonly IStudentCalculator _studentCalculator;
    private readonly IUserReader _userReader;
    PassTestHandler(IStudentWriter studentWriter, 
                    IStudentReader studentReader, 
                    IStudentCalculator studentCalculator, 
                    IUserReader userReader)
    {
        _studentWriter = studentWriter;
        _studentReader = studentReader;
        _studentCalculator = studentCalculator;
        _userReader = userReader;
    }

    public async Task<PassTestResult> Handle(PassTestCommand cmd)
    {
        //Authorization
        string Id = await _userReader.GetIdAsync(cmd.token);
        if(!await _studentReader.isStudent(Id))
        {
            throw new Exception(message: "You are not student");
        }

        //Validation
        //Do test rules are implemented here?

        //Fetch data
        TestAnswers realAnswers = await _studentReader.GetTestAnswersAsync(Id, cmd.Test);

        //Logic(Domain)
        TestAnswers studentAnswers = await _studentCalculator.GetStudentAnswersAsync(Id, cmd.Test);
        TestResult testResult = await _studentCalculator.CalculateTestResultAsync(Id, studentAnswers, realAnswers, cmd.Test.SecondarySubject1.Subject, cmd.Test.SecondarySubject2.Subject);
        TestResultClient res = new TestResultClient
        {
            TakenAt = testResult.TakenAt,
            SecondarySubject1 = testResult.SecondarySubject1,
            SecondarySubject2 = testResult.SecondarySubject2,
            KazakhHistoryScore = testResult.KazakhHistoryScore,
            FunctionalLiteracyScore = testResult.FunctionalLiteracyScore,
            MathematicalLiteracyScore = testResult.MathematicalLiteracyScore,
            SecondarySubject1Score = testResult.SecondarySubject1Score,
            SecondarySubject2Score = testResult.SecondarySubject2Score,
            TotalScore = testResult.TotalScore
        };
        //Persist
        await _studentWriter.SubmitTestAsync(Id, testResult);

        //Side effects

        //Return result
        return new PassTestResult(result: res);
    }
}