using API.Domain.Entities.QuestionTypes;
using API.Domain.Enums.Subject;

namespace API.Application.Interfaces.Users.IAdmin;

public interface IAdminReader
{
    Task<List<SingleChoiceQuestion>> GetSingleChoiceQuestionsAsync(string adminId, Subject subject, int docNumber);
    Task<List<MultipleChoiceQuestion>> GetMultipleChoiceQuestionsAsync(string adminId, Subject subject, int docNumber);
    Task<List<ContextQuestion>> GetContextQuestionsAsync(string adminId, Subject subject, int docNumber);
    Task<List<MatchQuestion>> GetMatchQuestionsAsync(string adminId, Subject subject, int docNumber);
    Task<int> GetSingleChoiceQuestionsDocumentSizeAsync(string adminId, Subject subject, int docNumber);
    Task<int> GetMultipleChoiceQuestionsDocumentSizeAsync(string adminId, Subject subject, int docNumber);
    Task<int> GetContextQuestionsDocumentSizeAsync(string adminId, Subject subject, int docNumber);
    Task<int> GetMatchQuestionsDocumentSizeAsync(string adminId, Subject subject, int docNumber);
    Task<bool> isAdmin(string Id);
}

public interface IAdminWriter
{
    Task AddSingleChoiceQuestionsAsync(string adminId, List<SingleChoiceQuestion> questions, Subject subject, int docNumber);
    Task AddMultipleChoiceQuestionsAsync(string adminId, List<MultipleChoiceQuestion> questions, Subject subject, int docNumber);
    Task AddContextQuestionsAsync(string adminId, List<ContextQuestion> questions, Subject subject, int docNumber);
    Task AddMatchQuestionsAsync(string adminId, List<MatchQuestion> questions, Subject subject, int docNumber);
}

public interface IAdminCalculator
{
    
}