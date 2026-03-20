using FirebaseAdmin.Auth;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

using API.Application.UseCases.Students.PassTest;
using API.Domain.Entities.Test;
using API;
using API.Domain.Enums.Subject;

namespace API.Controllers.PassTestController;

[ApiController]
[Route("api/[controller]")]
public class PassTestController: ControllerBase
{
    public readonly PassTestService _passTestService;
    public PassTestController(PassTestService passTestService)
    {
        _passTestService = passTestService;
    }
    [HttpPost("passtest")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<ActionResult<TestResultClient>> PassTest([FromBody] JsonElement body)
    {
        try
        {
            var authHeader = HttpContext.Request.Headers["Authorization"].ToString();
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            {
                return Unauthorized(new { Message = "Authorization header is missing or invalid" });
            }
            var token = authHeader.Substring("Bearer ".Length).Trim();
            Test test = JsonSerializer.Deserialize<Test>(body.GetRawText(), new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            TestResultClient result = await _passTestService.PassTest(token, test);
            var json = JsonSerializer.Serialize<TestResultClient>(result);
            return Content(json, "application/json");
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message, stackTrace = ex.ToString() });
        }
    }
}