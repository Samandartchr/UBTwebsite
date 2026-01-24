using Microsoft.AspNetCore.Mvc;
using API.Infrastructure.Database;
using FirebaseAdmin.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using API.Application.UseCases.Users.Register;
using API.Domain.Entities.User;

namespace API.Controllers.AuthController;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _dbContext;
    public readonly RegisterService _registerService;
    public readonly FirebaseAuth _firebaseAuth;

    public AuthController(AppDbContext dbContext, RegisterService registerService, FirebaseAuth firebaseAuth)
    {
        _dbContext = dbContext;
        _registerService = registerService;
        _firebaseAuth = firebaseAuth;
    }

    [HttpGet("checkuserexistence")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<ActionResult<bool>> CheckUserExistence()
    {
        try
        {
            var authHeader = HttpContext.Request.Headers["Authorization"].ToString();
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            {
                return Unauthorized(new { Message = "Authorization header is missing or invalid" });
            }

            var token = authHeader.Substring("Bearer ".Length).Trim();

            // Extract the Firebase user ID from the JWT token
            FirebaseToken decodedToken = await _firebaseAuth.VerifyIdTokenAsync(token);
            string firebaseUserId = decodedToken.Uid;

            if (string.IsNullOrEmpty(firebaseUserId))
            {
                return Unauthorized(new { message = "Invalid token: user_id claim is missing." });
            }

            // Check if the user exists in the database
            bool userExists = await _dbContext.Users.AnyAsync(u => u.Id == firebaseUserId);

            return userExists;
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while checking user existence.", error = ex.Message });
        }
    }

    [HttpPost("createuser")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> CreateUser([FromBody] UserRegister user)
    {
        try
        {
            var authHeader = HttpContext.Request.Headers["Authorization"].ToString();
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            {
                return Unauthorized(new { Message = "Authorization header is missing or invalid" });
            }

            var token = authHeader.Substring("Bearer ".Length).Trim();

            bool result = await _registerService.Register(user, token);

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while creating the user.", error = ex.Message });
        }
    }
}