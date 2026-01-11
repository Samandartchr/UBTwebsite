using API.Application.Interfaces.Users.IUser;
using API.Infrastructure.Persistence.SQLdatabase.SQLite.AppDbContext;
using API.Domain.Entities.User;
using API.Domain.Entities.Settings;
using API.Domain.Enums.UserRole;
using System;

namespace API.Infrastructure.Persistence.SQLdatabase.SQLite.UserRepo;

public class UserRepo: IUserReader, IUserWriter
{
    private readonly AppDbContext _context;
    public UserRepo(AppDbContext context)
    {
        _context = context;
    }

    //Reader methods
    public async Task<bool> isUsernameExist(string nickname)
    {
        bool exists;
        exists = await _context.Users.AnyAsync(u => u.Username == nickname);
        return exists;
    }
    public async Task<UserPublicInfo> GetUserPublicInfoAsync(string username)
    {
        User user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
        if (user == null) throw new Exception("User not found");

        return new UserPublicInfo
        {
            Username = user.Username,
            Email = user.Email,
            Role = Enum.Parse<UserRole>(user.Role),
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
            Role = user.Role.ToString(),
            Name = user.Name,
            Surname = user.Surname,
            CreatedAt = user.CreatedAt,
            PhoneNumber = user.PhoneNumber,
            ProfileImageLink = user.ProfileImageLink,
            isPremium = user.isPremium
        };

        _context.Users.Add(u);
        await _context.SaveChangesAsync();
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
    }
}