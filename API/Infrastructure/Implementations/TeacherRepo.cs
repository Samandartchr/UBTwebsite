using API.Infrastructure.Database;
using API.Application.Interfaces.Users.ITeacher;
using API.Domain.Enums.UserRole;
using API.Domain.Entities.User;
using Google.Cloud.Firestore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace API.Infrastructure.Implementations.TeacherRepository;

public class TeacherRepo: ITeacherReader, ITeacherWriter
{
    private readonly AppDbContext _context;
    private readonly FirestoreDb _db;
    public TeacherRepo(AppDbContext context, FirestoreDb db)
    {
        _context = context;
        _db = db;
    }

    // ITeacherReader implementation
    public async Task<List<GroupPublic>> GetTeacherGroupsAsync(string teacherId)
{
    return await _context.Groups
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

        // Delete group document from Firestore
        var GrDoc = _db.Collection("Groups").Document(groupId);
        await GrDoc.DeleteAsync();
    }

    public async Task InviteStudentToGroupAsync(string teacherId, string groupId, string studentId)
    {
        var group = await _context.Groups.FirstOrDefaultAsync(g => g.GroupId == groupId && g.TeacherId == teacherId);
        if (group != null)
        {
            if(await _context.GroupJoinOrders.AnyAsync(o => o.GroupId == groupId && o.SenderId == teacherId && o.AcceptorId == studentId))
            {
                throw new Exception("Invitation already sent");
            }
            if(await _context.GroupJoinOrders.AnyAsync(o => o.GroupId == groupId && o.SenderId == studentId && o.AcceptorId == teacherId))
            {
                throw new Exception("Student already sent invitation to teacher");
            }
            var invitation = new GroupJoinOrder
            {
                GroupId = groupId,
                SenderId = teacherId,
                AcceptorId = studentId
            };
            await _context.GroupJoinOrders.AddAsync(invitation);
           // _context.Database.ExecuteSqlRaw(
          //      "")
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
        // Add student to group in Firestore
        var GrDoc = _db.Collection("Groups").Document(groupId);
        var StDoc = _db.Collection("Students").Document(studentId);

        await GrDoc.UpdateAsync("Students", FieldValue.ArrayUnion(studentId));
        await StDoc.UpdateAsync("Groups", FieldValue.ArrayUnion(groupId));
    }

    public async Task CreateGroup(Group group)
    {
        _context.Groups.Add(group);
        await _context.SaveChangesAsync();

        // Create group document in Firestore
        var GrDoc = _db.Collection("Groups").Document(group.GroupId);
        await GrDoc.SetAsync(new Dictionary<string, object>
        {
            { "GroupName", group.GroupName },
            { "TeacherId", group.TeacherId },
            { "TeacherUsername", group.TeacherUsername },
            { "CreatedAt", group.CreatedAt },
            { "GroupDescription", group.GroupDescription },
            { "GroupImageLink", group.GroupImageLink }
        });
    }

    public async Task RemoveStudentFromGroupAsync(string teacherId, string groupId, string studentId)
    {
        var GrDoc = _db.Collection("Groups").Document(groupId);
        var StDoc = _db.Collection("Students").Document(studentId);

        //Remove an element of array in group document, the array is "Students", which contains Ids of students
        await GrDoc.UpdateAsync("Students", FieldValue.ArrayRemove(new Dictionary<string, object>
        {
            { "StudId", studentId }
        }));

        await StDoc.UpdateAsync("Groups", FieldValue.ArrayRemove(new Dictionary<string, object>
        {
            { "GroupId", groupId }
        }));

    }
}