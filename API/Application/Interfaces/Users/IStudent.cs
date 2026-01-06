using API.Domain.Entities.Test;
using API.Domain.Entities.User;
using API.Domain.Enums.Subject;

namespace API.Application.Interfaces.Users.IStudent;

public interface IStudentWriter
{
    Task JoinGroupAsync(string studentId, string groupId);
    Task AcceptGroupInviteAsync(string studentId, string groupId);
    Task SubmitTestAsync(string studentId, TestResult testResult);
}

public interface IStudentReader
{
    Task<bool> IsInGroupAsync(string studentId, string groupId);
    Task<List<string>> GetGroupInvitesAsync(string studentId);

    Task<Test> GetTestAsync(string studentId, Subject SecondarySubject1, Subject SecondarySubject2);
    Task<List<TestResult>> GetTestResultsAsync(string studentId);
    Task<TestAnswers> GetTestAnswersAsync(string studentId, Test test);
    Task<List<GroupPublic>> GetGroups(string studentId);

    
    Task<bool> isStudent(string Id);
}

public interface IStudentCalculator
{
    Task<TestAnswers> GetStudentAnswersAsync(string studentId, Test test);
    Task<TestResult> CalculateTestResultAsync(string studentId, 
                                            TestAnswers studentAnswers, 
                                            TestAnswers testAnswers, 
                                            Subject SecondarySubject1, 
                                            Subject SecondarySubject2);
}