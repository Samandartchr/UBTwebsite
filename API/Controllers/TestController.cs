using FirebaseAdmin.Auth;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

using API.Application.UseCases.Students.GetTest;
using API.Application.UseCases.Students.PassTest;
using API.Domain.Entities.Test;
using API;
using API.Domain.Enums.Subject;
using API.Application.Interfaces.Users.IStudent;
using API.Application.Interfaces.Users.IUser;

namespace API.Controllers.TestController;

[ApiController]
[Route("api/[controller]")]
public class TestController: ControllerBase
{
    public readonly GetTestService _getTestService;
    private readonly IStudentReader _studentReader;
    private readonly IUserReader _userReader;
    public TestController(GetTestService getTestService,
                          IStudentReader studentReader,
                          IUserReader userReader)
    {
        _getTestService = getTestService;
        _studentReader = studentReader;
        _userReader = userReader;
    }
    [HttpPost("gettest")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<ActionResult<Test>> GetTest([FromBody] JsonElement body)
    {
        try
        {
            var authHeader = HttpContext.Request.Headers["Authorization"].ToString();
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            {
                return Unauthorized(new { Message = "Authorization header is missing or invalid" });
            }
            var token = authHeader.Substring("Bearer ".Length).Trim();
            var subject1Str = body.GetProperty("subject1").GetString()?.Trim();
            var subject2Str = body.GetProperty("subject2").GetString()?.Trim();

            Enum.TryParse<Subject>(subject1Str, true, out Subject subject1);

            Enum.TryParse<Subject>(subject2Str, true, out Subject subject2) ;

            Test test = await _getTestService.GetTest(token, subject1, subject2); return test;
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("gettestresults")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<ActionResult<List<TestResultClient>>> GetTestResults()
    {
        try
        {
            var authHeader = HttpContext.Request.Headers["Authorization"].ToString();
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            {
                return Unauthorized(new { Message = "Authorization header is missing or invalid" });
            }
            var token = authHeader.Substring("Bearer ".Length).Trim();
            string userId = await _userReader.GetIdAsync(token);
            if (!await _studentReader.isStudent(userId))
                throw new Exception("You are not a student");

            // Fetch data
            List<TestResultClient> results = await _studentReader.GetTestResultsAsync(userId);

            // Return results
            return results;
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message, stackTrace = ex.ToString() });
        }
    }

    [HttpGet("getstudentresults/{username}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<ActionResult<List<TestResultClient>>> GetStudentResults(string username)
    {
        try
        {
            var authHeader = HttpContext.Request.Headers["Authorization"].ToString();
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            {
                return Unauthorized(new { Message = "Authorization header is missing or invalid" });
            }
            var token = authHeader.Substring("Bearer ".Length).Trim();
            string userId = await _userReader.GetIdAsync(token);
            // Fetch data
            var studId = await _userReader.GetIdByUsernameAsync(username);
            List<TestResultClient> results = await _studentReader.GetTestResultsAsync(studId);
            // Return results
            return results;
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message, stackTrace = ex.ToString() });
        }
    }
}