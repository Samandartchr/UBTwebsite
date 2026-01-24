using API.Domain.Entities.User;
using API.Application.Interfaces.Users.IUser;
using API.Application.Interfaces.Users.ITeacher;

namespace API.Application.UseCases.Teachers.GetGroups;

public class GetGroupsService
{
    private readonly ITeacherReader _teacherReader;
    private readonly IUserReader _userReader;

    public GetGroupsService(ITeacherReader teacherReader, IUserReader userReader)
    {
        _teacherReader = teacherReader;
        _userReader = userReader;
    }

    /// <summary>
    /// Returns all groups for a teacher. Throws exception if not a teacher.
    /// </summary>
    public async Task<List<GroupPublic>> GetGroups(string token)
    {
        // Authorization
        string userId = await _userReader.GetIdAsync(token);
        if (!await _teacherReader.isTeacher(userId))
            throw new Exception("You are not a teacher");

        // Fetch data
        List<GroupPublic> groups = await _teacherReader.GetTeacherGroupsAsync(userId);

        // Return result
        return groups;
    }
}
