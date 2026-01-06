using API.Domain.Entities.User;
using API.Application.Interfaces.Users.ITeacher;
using API.Application.Interfaces.Users.IUser;
using API.Domain.Rules.RegisterRules;

namespace API.Application.UseCases.Teachers.CreateGroup;

public record CreateGroupCommand(string token, Group group);
public record CreateGroupResult(bool isCreated);

public class CreateGroupHandler
{
    private readonly ITeacherWriter _teacherWriter;
    private readonly ITeacherReader _teacherReader;
    private readonly IUserReader _userReader;

    CreateGroupHandler(ITeacherWriter teacherWriter, ITeacherReader teacherReader, IUserReader userReader)
    {
        _teacherWriter = teacherWriter;
        _teacherReader = teacherReader;
        _userReader = userReader;
    }

    public async Task<CreateGroupResult> Handle(CreateGroupCommand cmd)
    {
        //Authorization
        string Id = await _userReader.GetIdAsync(cmd.token);
        if(!await _teacherReader.isTeacher(Id))
        {
            throw new Exception(message: "You are not teacher");
        }

        //Validation
        else if(await _teacherReader.isGroupExist(cmd.group.GroupId))
        {
            throw new Exception(message: "retry");
        }

        else if (RegisterValidator.isUsernameValid(cmd.group.GroupName))
        {
            throw new Exception(message: "Invalid group name");
        }

        //Fetch data
        

        //Fetch data

        //logic

        //Persist
        await _teacherWriter.CreateGroup(cmd.group);

        //Side effects

        //Return result
        return new CreateGroupResult(isCreated: true);
    }
}