using API.Domain.Entities.User;
using API.Application.Interfaces.Users.IGroup;
using API.Infrastructure.Database;
using Google.Cloud.Firestore;
using Microsoft.EntityFrameworkCore;
using API.Domain.Entities.Test;

namespace API.Infrastructure.Implementations.GroupRepository;

public class GroupRepo: IGroupReader, IGroupWriter
{
    private readonly AppDbContext _context;
    private readonly FirestoreDb _db;
    public GroupRepo(AppDbContext context, FirestoreDb db)
    {
        _context = context;
        _db = db;
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

        // Update Firestore document
        var GrDoc = _db.Collection("Groups").Document(Id);
        var updates = new Dictionary<string, object>
        {
            { "GroupDescription", settings.GroupDescription },
            { "GroupImageLink", settings.GroupImageLink }
        };
        await GrDoc.UpdateAsync(updates);
    }

    public async Task<List<UserPublicInfo>> GetGroupStudentsAsync(string Id, string groupId)
    {
        var GrDoc = _db.Collection("Groups").Document(groupId);
        string[] StudIDs = (await GrDoc.GetSnapshotAsync()).GetValue<string[]>("Students");
        List<UserPublicInfo> students = new List<UserPublicInfo>();
        foreach (var StudID in StudIDs)
        {
            // Fetch user data from the SQL database
            var user = await _context.Users.FindAsync(StudID);
            if (user != null)
            {
                students.Add(new UserPublicInfo
                {
                    Username = user.Username,
                    Email = user.Email,
                    Role = user.Role,
                    Name = user.Name,
                    Surname = user.Surname,
                    CreatedAt = user.CreatedAt,
                    PhoneNumber = user.PhoneNumber,
                    ProfileImageLink = user.ProfileImageLink,
                    isPremium = user.isPremium
                });
            }
            else
            {
                throw new Exception("User not found");
            }
        }
        return students;
    }

    public async Task<List<TestResultClient>> GetTestResultsGroupAsync(string Id, string groupId)
    {
        var GrDoc = _db.Collection("Groups").Document(groupId);
        string[] StudIDs = (await GrDoc.GetSnapshotAsync()).GetValue<string[]>("Students");
        List<TestResultClient> testResults = new List<TestResultClient>();
        foreach (var StudID in StudIDs)
        {
            // Fetch test results from SQL database
            var results = await _context.TestResults
                .Where(tr => tr.StudentId == StudID)
                .ToListAsync();
            testResults.AddRange(results);
        }
        return testResults;
    }
}