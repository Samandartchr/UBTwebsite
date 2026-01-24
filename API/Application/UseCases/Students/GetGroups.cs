using API.Domain.Entities.User;
using API.Application.Interfaces.Users.IUser;
using API.Application.Interfaces.Users.IStudent;

namespace API.Application.UseCases.Students.GetGroups;

public class GetGroupsService
{
    private readonly IStudentReader _studentReader;
    private readonly IUserReader _userReader;

    public GetGroupsService(IStudentReader studentReader, IUserReader userReader)
    {
        _studentReader = studentReader;
        _userReader = userReader;
    }

    /// <summary>
    /// Returns all groups a student belongs to. Throws if not a student.
    /// </summary>
    public async Task<List<GroupPublic>> GetGroups(string token)
    {
        // Authorization
        string userId = await _userReader.GetIdAsync(token);
        if (!await _studentReader.isStudent(userId))
            throw new Exception("You are not a student");

        // Fetch data
        List<GroupPublic> groups = await _studentReader.GetGroups(userId);

        // Return result
        return groups;
    }
}
