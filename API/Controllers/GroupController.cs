using Microsoft.AspNetCore.Mvc;
using API.Application.UseCases.Teachers.GetGroups;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using API.Domain.Entities.User;
using API.Application.UseCases.Teachers.CreateGroup;
using API.Application.Interfaces.Users.IUser;
using System.Security.Cryptography;
using FirebaseAdmin.Auth;
using System.Text.Json;
using API.Application.UseCases.Students;
using API;
using API.Application.Interfaces.Users.IGroup;
using API.Domain.Entities.Test;
using API.Application.Interfaces.Users.IStudent;

namespace API.Controllers.GroupController;

[ApiController]
[Route("api/[controller]")]
public class GroupController: ControllerBase
{
    public readonly GetGroupsService _getGroupsService;
    public readonly CreateGroupService _createGroupService;
    public readonly IUserReader _userReader;
    public readonly FirebaseAuth _firebaseAuth;
    private readonly IGroupReader _groupReader;
    private readonly IStudentReader _studentReader;

    public GroupController(GetGroupsService getGroupsService,
                    CreateGroupService createGroupService,
                    IUserReader userReader,
                    FirebaseAuth firebaseAuth,
                    IGroupReader groupReader,
                    IStudentReader studentReader)
    {
        _getGroupsService = getGroupsService;
        _createGroupService = createGroupService;
        _userReader = userReader;
        _firebaseAuth = firebaseAuth;
        _groupReader = groupReader;
        _studentReader = studentReader;
    }

    [HttpGet("getteachergroups")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<ActionResult<List<GroupPublic>>> GetTeacherGroups()
    {
        try
        {
            var authHeader = HttpContext.Request.Headers["Authorization"].ToString();
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            {
                return Unauthorized(new { Message = "Authorization header is missing or invalid" });
            }

            var token = authHeader.Substring("Bearer ".Length).Trim();

            List<GroupPublic> groups = await _getGroupsService.GetGroups(token);
            return groups;
        }

        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("creategroup")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<ActionResult<string>> CreateGroup([FromBody] JsonElement body)
    {
        try
        {
            var authHeader = HttpContext.Request.Headers["Authorization"].ToString();
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            {
                return Unauthorized(new { Message = "Authorization header is missing or invalid" });
            }

            var token = authHeader.Substring("Bearer ".Length).Trim();
            var groupname = body.GetProperty("groupname").GetString();

            const string chars = "abcdefghijklmnopqrstuvwxyz0123456789";
            string groupId = new string(Enumerable.Range(0, 8)
                .Select(_ => chars[RandomNumberGenerator.GetInt32(chars.Length)])
                .ToArray());
            string Id = await _userReader.GetIdAsync(token);
            string TeacherUsername = await _userReader.GetUsernameAsync(Id);
            Group group = new Group
            {
                GroupId = groupId,
                GroupName = groupname,
                TeacherUsername = TeacherUsername,
                CreatedAt = DateTime.UtcNow,
                TeacherId = Id
            };

            bool res = await _createGroupService.CreateGroup(token, group);
            if (res) return groupId;
            else return BadRequest(new { message = "Failed to create group" });
        }
        catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpGet("getjoinedgroups")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<ActionResult<List<GroupPublic>>> GetJoinedGroups()
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
            List<GroupPublic> groups = await _studentReader.GetStudentGroups(userId);

            // Return result
            return groups;
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("getgroupresults/{groupId}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<ActionResult<List<TestResultClient>>> GetGroupResults(string groupId)
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

            // Validation
            if (!await _groupReader.isGroupExist(groupId))
                throw new Exception("Group does not exist");

            // Fetch data
            List<TestResultClient> results = await _groupReader.GetTestResultsGroupAsync(userId, groupId);

            return results;
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("getgroupinfo/{groupId}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<ActionResult<GroupPublic>> GetGroupInfo(string groupId)
    {
        try
        {
            var authHeader = HttpContext.Request.Headers["Authorization"].ToString();
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            {
                return Unauthorized(new { Message = "Authorization header is missing or invalid" });
            }
            var token = authHeader.Substring("Bearer ".Length).Trim();
            // Validation
            string userId = await _userReader.GetIdAsync(token);

            // Validation
            if (!await _groupReader.isGroupExist(groupId))
                throw new Exception("Group does not exist");

            // Fetch data
            GroupPublic group = await _groupReader.GetGroupAsync(groupId);

            return group;
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("getstudents/{groupId}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<ActionResult<List<UserPublicInfo>>> GetStudents(string groupId)
    {
        try
        {
            var authHeader = HttpContext.Request.Headers["Authorization"].ToString();
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            {
                return Unauthorized(new { Message = "Authorization header is missing or invalid" });
            }
            var token = authHeader.Substring("Bearer ".Length).Trim();
            // Validation
            string userId = await _userReader.GetIdAsync(token);

            // Validation
            if (!await _groupReader.isGroupExist(groupId))
                throw new Exception("Group does not exist");

            // Fetch data
            List<UserPublicInfo> students = await _groupReader.GetGroupStudentsAsync(userId, groupId);

            return students;
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}