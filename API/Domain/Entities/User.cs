using API.Domain.Enums.Subject;
using API.Domain.Enums.UserRole;

namespace API.Domain.Entities.User;

public class UserRegister
{
    public required string Username { get; set; }
    public required string Email { get; set; }
    public required UserRole Role { get; set; }
    public required string Name { get; set; }
    public required string Surname { get; set; }
}

public class UserPublicInfo: UserRegister
{
    public required DateTime CreatedAt { get; set; }
    public string? PhoneNumber { get; set; }
    public string? ProfileImageLink { get; set; }
    public required bool isPremium { get; set; } = false;
}

public class User: UserPublicInfo
{
    public required string Id { get; set; }
    
}

public class Student: User
{
    public List<string>? GroupsLinks {get; set;}
    public DateTime? LastTimeTest {get; set;}
}

public class Teacher: User
{
    public List<string>? GroupsLinks {get; set;}
}

public class Admin: User
{
    public required bool isAllowed {get; set;} = false;
}

public class GroupBase
{
    public string? GroupDescription {get; set;}
    public string? GroupImageLink {get; set;}
}

public class GroupSettings: GroupBase
{
    //Later
}

public class GroupPublic: GroupBase
{
    public required string GroupId {get; set;}
    public required string GroupName {get; set;}
    public required string TeacherUsername {get; set;}
    public required DateTime CreatedAt {get; set;}
}

public class Group: GroupPublic
{
    public required string TeacherId {get; set;}
}