using API.Domain.Entities.Test;
using API.Domain.Entities.User;

namespace API.Application.Interfaces.Users.ITeacher;

public interface ITeacherReader
{
    Task<List<GroupPublic>> GetTeacherGroupsAsync(string teacherId);
    Task<bool> isTeacher(string Id);
}

public interface ITeacherWriter
{
    Task DeleteGroup(string teacherId, string groupId);

    Task InviteStudentToGroupAsync(string teacherId, string groupId, string studentId);
    Task AcceptStudentToGroupAsync(string teacherId, string groupId, string studentId);
    Task RemoveStudentFromGroupAsync(string teacherId, string groupId, string studentId);//firebase
    
    Task CreateGroup(Group group);
}