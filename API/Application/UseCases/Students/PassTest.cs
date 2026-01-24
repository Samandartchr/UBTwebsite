using API.Application.Interfaces.Users.IStudent;
using API.Application.Interfaces.Users.IUser;
using API.Domain.Entities.Test;

namespace API.Application.UseCases.Students.PassTest;

public class PassTestService
{
    private readonly IStudentWriter _studentWriter;
    private readonly IStudentReader _studentReader;
    private readonly IStudentCalculator _studentCalculator;
    private readonly IUserReader _userReader;

    public PassTestService(
        IStudentWriter studentWriter,
        IStudentReader studentReader,
        IStudentCalculator studentCalculator,
        IUserReader userReader)
    {
        _studentWriter = studentWriter;
        _studentReader = studentReader;
        _studentCalculator = studentCalculator;
        _userReader = userReader;
    }

    /// <summary>
    /// Submits a student's test, calculates results, and persists them.
    /// Throws if user is not a student.
    /// </summary>
    public async Task<TestResultClient> PassTest(string token, Test test)
    {
        // Authorization
        string userId = await _userReader.GetIdAsync(token);
        if (!await _studentReader.isStudent(userId))
            throw new Exception("You are not a student");

        // Fetch actual answers
        TestAnswers realAnswers = await _studentReader.GetTestAnswersAsync(userId, test);

        // Fetch student's answers
        TestAnswers studentAnswers = await _studentCalculator.GetStudentAnswersAsync(userId, test);

        // Calculate test result
        TestResult testResult = await _studentCalculator.CalculateTestResultAsync(
            userId, studentAnswers, realAnswers,
            test.SecondarySubject1.Subject,
            test.SecondarySubject2.Subject
        );

        TestResultClient resultClient = new TestResultClient
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

        // Persist results
        await _studentWriter.SubmitTestAsync(userId, testResult);

        return resultClient;
    }
}
