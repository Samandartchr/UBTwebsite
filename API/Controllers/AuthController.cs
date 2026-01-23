using Microsoft.AspNetCore.Mvc;
using API.Infrastructure.Database;
using FirebaseAdmin.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace API.Controllers.AuthController;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _dbContext;
    
    public AuthController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet("checkuserexistence")]
    public async Task<IActionResult> CheckUserExistence()
    {
        try
        {
            // Extract the Firebase user ID from the JWT token
            var firebaseUserId = User.FindFirst("user_id")?.Value;

            if (string.IsNullOrEmpty(firebaseUserId))
            {
                return Unauthorized(new { message = "Invalid token: user_id claim is missing." });
            }

            // Check if the user exists in the database
            var userExists = await _dbContext.Users.AnyAsync(u => u.Id == firebaseUserId);

            return Ok(new { exists = userExists });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while checking user existence.", error = ex.Message });
        }
    }
}