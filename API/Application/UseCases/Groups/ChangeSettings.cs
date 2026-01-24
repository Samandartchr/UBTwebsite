using API.Domain.Entities.User;
using API.Application.Interfaces.Users.IUser;
using API.Application.Interfaces.Users.ITeacher;
using API.Application.Interfaces.Users.IGroup;

namespace API.Application.UseCases.Groups.ChangeSettings;

public class ChangeSettingsService
{
    private readonly IUserReader _userReader;
    private readonly ITeacherReader _teacherReader;
    private readonly IGroupWriter _groupWriter;
    private readonly IGroupReader _groupReader;

    public ChangeSettingsService(
        IUserReader userReader,
        ITeacherReader teacherReader,
        IGroupWriter groupWriter,
        IGroupReader groupReader)
    {
        _userReader = userReader;
        _teacherReader = teacherReader;
        _groupWriter = groupWriter;
        _groupReader = groupReader;
    }

    /// <summary>
    /// Changes the settings of a group if the user is a teacher and the group exists.
    /// </summary>
    public async Task ChangeGroupSettings(string token, string groupId, GroupSettings settings)
    {
        // Authorization
        string userId = await _userReader.GetIdAsync(token);
        if (!await _teacherReader.isTeacher(userId))
            throw new Exception("You are not a teacher");

        // Validation
        if (!await _groupReader.isGroupExist(groupId))
            throw new Exception("Group does not exist");

        // Persist changes
        await _groupWriter.ChangeGroupSettings(groupId, settings);
    }
}
