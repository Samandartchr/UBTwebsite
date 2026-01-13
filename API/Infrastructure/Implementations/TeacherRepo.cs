using API.Infrastructure.Database;
using API.Application.Interfaces.Users.ITeacher;
using API.Domain.Enums.UserRole;
using API.Domain.Entities.User;

namespace API.Infrastructure.Implementations.TeacherRepository;

public class TeacherRepo: ITeacherReader, ITeacherWriter
{
    private readonly AppDbContext _context;
    public TeacherRepo(AppDbContext context)
    {
        _context = context;
    }

    // ITeacherReader implementation
    public async Task<List<GroupPublic>> GetTeacherGroupsAsync(string teacherId)
    {
        List<GroupPublic> groups = new List<GroupPublic>();
        groups = await _context.Groups
            .Where(g => g.TeacherId == teacherId)
            .Select(g => new GroupPublic
            {
                GroupId = g.GroupId,
                GroupName = g.GroupName,
                TeacherUsername = g.TeacherUsername,
                CreatedAt = g.CreatedAt,
                GroupDescription = g.GroupDescription,
                GroupImageLink = g.GroupImageLink
            })
            .ToListAsync();
    }

    public async Task<bool> isTeacher(string Id)
    {
        var user = await _context.Users.FindAsync(Id);
        return user != null && user.Role == UserRole.Teacher;
    }

    // ITeacherWriter implementation
    public async Task DeleteGroup(string teacherId, string groupId)
    {
        var group = await _context.Groups.FirstOrDefaultAsync(g => g.GroupId == groupId && g.TeacherId == teacherId);
        if (group != null)
        {
            _context.Groups.Remove(group);
            await _context.SaveChangesAsync();
        }
    }

    public async Task InviteStudentToGroupAsync(string teacherId, string groupId, string studentId)
    {
        var group = await _context.Groups.FirstOrDefaultAsync(g => g.GroupId == groupId && g.TeacherId == teacherId);
        if (group != null)
        {
            var invitation = new GroupJoinOrder
            {
                GroupId = groupId,
                SenderId = teacherId,
                AcceptorId = studentId
            };
            _context.GroupJoinOrders.Add(invitation);
            await _context.SaveChangesAsync();
        }
    }

    public async Task AcceptStudentToGroupAsync(string teacherId, string groupId, string studentId)
    {
        var invitation = await _context.GroupJoinOrders
            .FirstOrDefaultAsync(o => o.GroupId == groupId && o.SenderId == studentId && o.AcceptorId == teacherId);
        if (invitation != null)
        {
            _context.GroupJoinOrders.Remove(invitation);
            await _context.SaveChangesAsync();
        }
    }

    public async Task CreateGroup(Group group)
    {
        _context.Groups.Add(group);
        await _context.SaveChangesAsync();
    }
}