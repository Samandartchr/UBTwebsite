

namespace API.Application.Interfaces.Common.IStatistics;

public interface IStatisticsReader
{
    Task<int> GetTotalUsersAsync();
    Task<int> GetTotalQuestionsAsync();
    Task<int> GetTotalGroupsAsync();
}