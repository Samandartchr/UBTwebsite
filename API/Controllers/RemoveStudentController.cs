using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Security.Cryptography;
using FirebaseAdmin.Auth;
using System.Text.Json;
using API.Application.Interfaces.Users.ITeacher;
using API.Application.Interfaces.Users.IUser;
using API.Application.Interfaces.Users.IGroup;

[ApiController]
[Route("api/[controller]")]
public class RemoveStudentController: ControllerBase
{
    private readonly ITeacherWriter _teacherWriter;
    private readonly ITeacherReader _teacherReader;
    private readonly IUserReader _userReader;
    private readonly IGroupReader _groupReader;

    public RemoveStudentController(
        ITeacherWriter teacherWriter,
        ITeacherReader teacherReader,
        IUserReader userReader,
        IGroupReader groupReader)
    {
        _teacherWriter = teacherWriter;
        _teacherReader = teacherReader;
        _userReader = userReader;
        _groupReader = groupReader;
    }
    [HttpPost("removestudent")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    //from Body: { "groupId": "...", "studentUsername": "..." }
    public async Task<IActionResult> RemoveStudent([FromBody] JsonElement body)
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

            if (!await _teacherReader.isTeacher(userId))
                throw new Exception("You are not a teacher");

            // extract from body
            string groupId = body.GetProperty("groupId").GetString();
            string studentUsername = body.GetProperty("studentUsername").GetString();

            // Validation
            if (!await _groupReader.isGroupExist(groupId))
                throw new Exception("Group not found");

            // Fetch data
            string studentId = await _userReader.GetIdByUsernameAsync(studentUsername);

            // Persist
            await _teacherWriter.RemoveStudentFromGroupAsync(userId, groupId, studentId);

            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message, stackTrace = ex.ToString() });
        }
    }
}