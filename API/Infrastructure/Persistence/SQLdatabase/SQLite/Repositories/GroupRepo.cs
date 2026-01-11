using API.Domain.Entities.User;
using API.Application.Interfaces.Users.IGroup;
using API.Infrastructure.Persistence.SQLdatabase.SQLite.AppDbContext;

namespace API.Infrastructure.Persistence.SQLdatabase.SQLite.GroupRepo;

public class GroupRepo: IGroupReader, IGroupWriter
{
    private readonly AppDbContext _context;
    public GroupRepo(AppDbContext context)
    {
        _context = context;
    }

    // IGroupReader implementation
    public async Task<GroupPublic> GetGroupAsync(string Id)
    {
        var group = await _context.Groups.FindAsync(Id);
        if (group == null) throw new Exception("Group not found");

        return new GroupPublic
        {
            GroupId = group.GroupId,
            GroupName = group.GroupName,
            TeacherUsername = group.TeacherUsername,
            CreatedAt = group.CreatedAt,
            GroupDescription = group.GroupDescription,
            GroupImageLink = group.GroupImageLink
        };
    }

    public async Task<bool> isGroupExist(string groupId)
    {
        return await _context.Groups.AnyAsync(g => g.GroupId == groupId);
    }

    public async Task ChangeGroupSettings(string Id, GroupSettings settings)
    {
        var group = await _context.Groups.FindAsync(Id);
        if (group == null) throw new Exception("Group not found");

        group.GroupDescription = settings.GroupDescription;
        group.GroupImageLink = settings.GroupImageLink;

        await _context.SaveChangesAsync();
    }
}