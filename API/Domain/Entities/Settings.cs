namespace API.Domain.Entities.Settings;

public class Settings
{
    public required string Name { get; set; }
    public required string Surname { get; set; }
    public string? PhoneNumber { get; set; }
    public string? ProfileImageLink { get; set; }
}