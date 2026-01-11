using API.Domain.Entities.User;
using API.Domain.Entities.Test;

namespace API.Application.Interfaces.Users.IGroup;

public interface IGroupReader
{
    Task<GroupPublic> GetGroupAsync(string Id);
    Task<List<UserPublicInfo>> GetGroupStudentsAsync(string Id, string groupId);//firebase
    Task<List<TestResultClient>> GetTestResultsGroupAsync(string Id, string groupId);//firebase
    Task<bool> isGroupExist(string groupId);
}

public interface IGroupWriter
{
    Task ChangeGroupSettings(string Id, GroupSettings settings);
}