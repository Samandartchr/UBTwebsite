using API.Domain.Entities.User;
using API.Application.Interfaces.Users.IUser;
using API.Application.Interfaces.Users.ITeacher;
using API.Application.Interfaces.Users.IGroup;

namespace API.Application.UseCases.Groups.ChangeSettings;

public record ChangeSettingsCommand(string token, string groupId, GroupSettings settings);
public record ChangeSettingsResult();

public class ChangeSettingsHandler
{
    private readonly IUserReader _userReader;
    private readonly ITeacherReader _teacherReader;
    private readonly IGroupWriter _groupWriter;
    private readonly IGroupReader _groupReader;


    ChangeSettingsHandler(IUserReader userReader, ITeacherReader teacherReader, IGroupWriter groupWriter, IGroupReader groupReader)
    {
        _userReader = userReader;
        _teacherReader = teacherReader;
        _groupWriter = groupWriter;
        _groupReader = groupReader;
    }
    public async Task<ChangeSettingsResult> Handle(ChangeSettingsCommand cmd)
    {
        //Authorization
        string Id = await _userReader.GetIdAsync(cmd.token);
        if(!await _teacherReader.isTeacher(Id))
        {
            throw new Exception(message: "You are not a teacher");
        }
        //Validation
        if(!await _groupReader.isGroupExist(cmd.groupId))
        {
            throw new Exception(message: "Group does not exist");
        }
        //Fetch data

        //Logic

        //Persist
        await _groupWriter.ChangeGroupSettings(cmd.groupId, cmd.settings);

        //Side effects

        //Return result
        return new ChangeSettingsResult();
    }
}