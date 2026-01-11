using API.Domain.Enums.Subject;
using API.Domain.Enums.UserRole;
using Microsoft.EntityFrameworkCore;

namespace API.Domain.Entities.User;

public class UserRegister
{
    [Column("username")]
    public required string Username { get; set; }
    [Column("email")]
    public required string Email { get; set; }
    [Column("role")]
    public required UserRole Role { get; set; }
    [Column("name")]
    public required string Name { get; set; }
    [Column("surname")]
    public required string Surname { get; set; }
}

public class UserPublicInfo: UserRegister
{
    [Column("created_at")]
    public required DateTime CreatedAt { get; set; }
    [Column("phone_number")]
    public string? PhoneNumber { get; set; }
    [Column("profile_image_link")]
    public string? ProfileImageLink { get; set; }
    [Column("is_premium")]
    public required bool isPremium { get; set; } = false;
}

[Table("users")]
public class User: UserPublicInfo
{
    [Column("user_id")]
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
    [Column("description")]
    public string? GroupDescription {get; set;}
    [Column("profile_image_link")]
    public string? GroupImageLink {get; set;}
}

public class GroupSettings: GroupBase
{
    //Later
}

public class GroupPublic: GroupBase
{
    [Column("group_id")]
    public required string GroupId {get; set;}
    [Column("group_name")]
    public required string GroupName {get; set;}
    [Column("teacher_username")]
    public required string TeacherUsername {get; set;}
    [Column("created_at")]
    public required DateTime CreatedAt {get; set;}
}

[Table("groups")]
public class Group: GroupPublic
{
    [Column("teacher_id")]
    public required string TeacherId {get; set;}
}
[Table("group_join_orders")]
public class GroupJoinOrder
{
    [Column("group_id")]
    public required string GroupId {get; set;}
    [Column("sender_id")]
    public required string SenderId {get; set;}
    [Column("acceptor_id")]
    public required string AcceptorId {get; set;}
}