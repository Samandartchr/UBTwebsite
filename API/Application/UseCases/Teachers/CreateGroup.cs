using API.Domain.Entities.User;
using API.Application.Interfaces.Users.ITeacher;
using API.Application.Interfaces.Users.IUser;
using API.Application.Interfaces.Users.IGroup;
using API.Domain.Rules.RegisterRules;

namespace API.Application.UseCases.Teachers.CreateGroup;

public class CreateGroupService
{
    private readonly ITeacherWriter _teacherWriter;
    private readonly ITeacherReader _teacherReader;
    private readonly IUserReader _userReader;
    private readonly IGroupReader _groupReader;

    public CreateGroupService(
        ITeacherWriter teacherWriter,
        ITeacherReader teacherReader,
        IUserReader userReader,
        IGroupReader groupReader)
    {
        _teacherWriter = teacherWriter;
        _teacherReader = teacherReader;
        _userReader = userReader;
        _groupReader = groupReader;
    }

    /// <summary>
    /// Creates a new group. Throws exception if invalid.
    /// </summary>
    public async Task<bool> CreateGroup(string token, Group group)
    {
        // Authorization
        string userId = await _userReader.GetIdAsync(token);
        if (!await _teacherReader.isTeacher(userId))
            throw new Exception("You are not a teacher");

        // Validation
        if (await _groupReader.isGroupExist(group.GroupId))
            throw new Exception("Group already exists");

        if (!RegisterValidator.isUsernameValid(group.GroupName))
            throw new Exception("Invalid group name");

        // Logic / Domain

        // Persist
        await _teacherWriter.CreateGroup(group);

        // Side effects (if any)

        return true;
    }
}
