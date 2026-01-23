using API.Application.Interfaces.Users.IUser;
using API.Infrastructure.Database;
using API.Domain.Entities.User;
using API.Domain.Entities.Settings;
using API.Domain.Enums.UserRole;
using FirebaseAdmin;
using Google.Cloud.Firestore;
using FirebaseAdmin.Auth;
using Microsoft.EntityFrameworkCore;
namespace API.Infrastructure.Implementations.UserRepository;

public class UserRepo: IUserReader, IUserWriter
{
    private readonly AppDbContext _context;
    private readonly FirestoreDb _db;
    private readonly FirebaseAuth _auth;
    public UserRepo(AppDbContext context, FirestoreDb db, FirebaseAuth auth)
    {
        _context = context;
        _db = db;
        _auth = auth;
    }

    //Reader methods
    public async Task<string> GetIdAsync(string token)
    {
        FirebaseToken decodedToken = await _auth.VerifyIdTokenAsync(token);
        return decodedToken.Uid;
    }
    public async Task<bool> isUsernameExist(string nickname)
    {
        bool exists;
        exists = await _context.Users.AnyAsync(user => user.Username == nickname);
        return exists;
    }
    public async Task<UserPublicInfo> GetUserPublicInfoAsync(string username)
    {
        User? user = await _context.Users.FirstOrDefaultAsync(user => user.Username == username);
        if (user == null) throw new Exception("User not found");

        return new UserPublicInfo
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
        };
    }
    public async Task<string> GetUsernameAsync(string userId)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

        return user?.Username ?? throw new Exception("User not found");
    }
    public async Task<string> GetIdByUsernameAsync(string username)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);

        return user?.Id ?? throw new Exception("User not found");
    }

    //Writer methods
    public async Task AddUser(User user)
    {
        var u = new User
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            Role = user.Role,
            Name = user.Name,
            Surname = user.Surname,
            CreatedAt = user.CreatedAt,
            PhoneNumber = user.PhoneNumber,
            ProfileImageLink = user.ProfileImageLink,
            isPremium = user.isPremium
        };

        _context.Users.Add(u);
        await _context.SaveChangesAsync();

        //Add user document to Firestore
        DocumentReference userDoc;
        if (user.Role.ToString() == "Student")
        {
            userDoc = _db.Collection("Students").Document(user.Id);
        }
        else
        {
            userDoc = _db.Collection("Teachers").Document(user.Id);
        }
        await userDoc.SetAsync(new
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            Role = user.Role.ToString(),
            Name = user.Name,
            Surname = user.Surname,
            CreatedAt = user.CreatedAt,
            PhoneNumber = user.PhoneNumber,
            ProfileImageLink = user.ProfileImageLink,
            isPremium = user.isPremium,
            LastTimeTest = DateTime.UtcNow
        });
    }
    public async Task ChangeSettings(string userId, Settings settings)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null) throw new Exception("User not found");

        user.Name = settings.Name;
        user.Surname = settings.Surname;
        user.PhoneNumber = settings.PhoneNumber;
        user.ProfileImageLink = settings.ProfileImageLink;

        await _context.SaveChangesAsync();

        //Update Firestore document
        DocumentReference userDoc;
        if (user.Role.ToString() == "Student")
        {
            userDoc = _db.Collection("Students").Document(user.Id);
        }
        else
        {
            userDoc = _db.Collection("Teachers").Document(user.Id);
        }
        await userDoc.UpdateAsync(new Dictionary<string, object>
        {
            { "Name", settings.Name },
            { "Surname", settings.Surname },
            { "PhoneNumber", settings.PhoneNumber },
            { "ProfileImageLink", settings.ProfileImageLink }
        });
    }
}