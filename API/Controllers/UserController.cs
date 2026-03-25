using API.Application.UseCases.Users.ChangeSettings;
using FirebaseAdmin.Auth;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using API.Domain.Entities.Settings;
using API.Domain.Entities.User;
using API.Application.Interfaces.Users.IUser;

namespace API.Controllers.UserController;

[ApiController]
[Route("api/[controller]")]
public class UserController: ControllerBase
{
    private readonly ChangeSettingsService _changeSettingsService;
    private readonly IUserReader _userReader;
    public UserController(ChangeSettingsService changeSettingsService, IUserReader userReader)
    {
        _changeSettingsService = changeSettingsService;
        _userReader = userReader;
    }
    [HttpPost("changesettings")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<ActionResult<UserPublicInfo>> ChangeSettings([FromBody] JsonElement body)
    {
        try
        {
            var authHeader = HttpContext.Request.Headers["Authorization"].ToString();
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            {
                return Unauthorized(new { Message = "Authorization header is missing or invalid" });
            }
            var token = authHeader.Substring("Bearer ".Length).Trim();
            Settings settings = new Settings
            {
                Name = body.GetProperty("name").GetString() ?? throw new Exception("Name is required"),
                Surname = body.GetProperty("surname").GetString() ?? throw new Exception("Surname is required"),
                PhoneNumber = body.TryGetProperty("phone", out JsonElement phoneNumberElement) ? phoneNumberElement.GetString() : null,
                ProfileImageLink = body.TryGetProperty("profileImageLink", out JsonElement profileImageLinkElement) ? profileImageLinkElement.GetString() : null
            };
            await _changeSettingsService.ChangeSettings(token, settings);
            UserPublicInfo info = await _userReader.GetUserPublicInfoAsync(await _userReader.GetUsernameAsync(await _userReader.GetIdAsync(token)));
            return info;
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    
}