using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using FirebaseAdmin.Auth;
using API.Application.UseCases.Teachers.InviteStudent;
using API.Infrastructure.Database;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using API.Domain.Entities.User;
using API.Application.Interfaces.Users.IUser;
using Microsoft.EntityFrameworkCore;
using API.Application.UseCases.Students.AcceptInvite;
using API.Application.UseCases.Students.JoinGroup;
using API.Application.UseCases.Teachers.AcceptStudent;
using System.ComponentModel;

namespace API.Controllers.MembershipController;

[ApiController]
[Route("api/[controller]")]
public class MembershipController: ControllerBase
{
    public readonly InviteStudentService _inviteStudentService;
    public readonly AppDbContext _db;
    public readonly IUserReader _userReader;
    public readonly AcceptInviteService _acceptInviteService;
    public readonly JoinGroupService _joinGroupService;
    public readonly AcceptStudentService _acceptStudentService;

    public MembershipController(InviteStudentService inviteStudentService, 
                                AppDbContext db,
                                IUserReader userReader,
                                AcceptInviteService acceptInviteService,
                                JoinGroupService joinGroupService,
                                AcceptStudentService acceptStudentService)
    {
        _inviteStudentService = inviteStudentService;
        _db = db;
        _userReader = userReader;
        _acceptInviteService = acceptInviteService;
        _joinGroupService = joinGroupService;
        _acceptStudentService = acceptStudentService;
    }
    //Teacher sends
    [HttpPost("sendinvitation")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<ActionResult<bool>> SendInvitation([FromBody] JsonElement body)
    {
        try
        {
            var authHeader = HttpContext.Request.Headers["Authorization"].ToString();
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            {
                return Unauthorized(new { Message = "Authorization header is missing or invalid" });
            }

            var token = authHeader.Substring("Bearer ".Length).Trim();

            string groupId = body.GetProperty("groupId").GetString();
            string student = body.GetProperty("nickname").GetString();

            await _inviteStudentService.InviteStudent(token, groupId, student);

            return true;
        }
        catch (Exception ex){return BadRequest(new { message = ex.Message });}
    }

    [HttpGet("getinvitations")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<ActionResult<List<GroupJoinOrderPublic>>> GetInvitations()
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

            //fetch from DB, GroupJoinOrders where user is sender
            List<GroupJoinOrderPublic> res = new List<GroupJoinOrderPublic>();
            res = await _db.GroupJoinOrders.Where(g => g.SenderId == userId).
                Select(g => new GroupJoinOrderPublic
                {
                    Id = g.Id,
                    GroupId = g.GroupId,
                    GroupName = _db.Groups.Where(gr => gr.GroupId == g.GroupId).Select(gr => gr.GroupName).FirstOrDefault(),
                    AcceptorUsername = _db.Users.Where(u => u.Id == g.AcceptorId).Select(u => u.Username).FirstOrDefault(),
                    SenderUsername = _db.Users.Where(u => u.Id == g.SenderId).Select(u => u.Username).FirstOrDefault()
                }).ToListAsync();
            return res;
        }
        catch (Exception ex){return BadRequest(new { message = ex.Message }); }
    }

    [HttpGet("getrequests")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<ActionResult<List<GroupJoinOrderPublic>>> GetRequests()
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
            //fetch from DB, GroupJoinOrders where user is acceptor
            List<GroupJoinOrderPublic> res = new List<GroupJoinOrderPublic>();
            res = await _db.GroupJoinOrders.Where(g => g.AcceptorId == userId).
                Select(g => new GroupJoinOrderPublic
                {
                    Id = g.Id,
                    GroupId = g.GroupId,
                    GroupName = _db.Groups.Where(gr => gr.GroupId == g.GroupId).Select(gr => gr.GroupName).FirstOrDefault(),
                    AcceptorUsername = _db.Users.Where(u => u.Id == g.AcceptorId).Select(u => u.Username).FirstOrDefault(),
                    SenderUsername = _db.Users.Where(u => u.Id == g.SenderId).Select(u => u.Username).FirstOrDefault()
                }).ToListAsync();
            return res;
        }
        catch (Exception ex){return BadRequest(new { message = ex.Message }); }
    }

    [HttpPost("acceptasstudent")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<ActionResult<bool>> AcceptAsStudent([FromBody] JsonElement body)
    {
        try
        {
            var authHeader = HttpContext.Request.Headers["Authorization"].ToString();
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            {
                return Unauthorized(new { Message = "Authorization header is missing or invalid" });
            }
            var token = authHeader.Substring("Bearer ".Length).Trim();
            string groupId = body.GetProperty("groupId").GetString();
            await _acceptInviteService.AcceptInvite(token, groupId);
            return true;
        }
        catch (Exception ex){return BadRequest(new { message = ex.Message }); }
    }
    //student sends
    [HttpPost("sendrequest")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<ActionResult<bool>> SendRequest([FromBody] JsonElement body)
    {
        try
        {
            var authHeader = HttpContext.Request.Headers["Authorization"].ToString();
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            {
                return Unauthorized(new { Message = "Authorization header is missing or invalid" });
            }
            var token = authHeader.Substring("Bearer ".Length).Trim();
            string groupId = body.GetProperty("groupId").GetString();
            await _joinGroupService.JoinGroup(token, groupId);
            return true;
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("acceptstudent")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<ActionResult<bool>> AcceptAsTeacher([FromBody] JsonElement body)
    {
        try
        {
            var authHeader = HttpContext.Request.Headers["Authorization"].ToString();
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            {
                return Unauthorized(new { Message = "Authorization header is missing or invalid" });
            }
            var token = authHeader.Substring("Bearer ".Length).Trim();
            string groupId = body.GetProperty("groupId").GetString();
            string studentUsername = body.GetProperty("username").GetString();
            //throw an exeption if the request is not found
            var acceptorId = await _userReader.GetIdAsync(token);
            var senderId = await _userReader.GetIdByUsernameAsync(studentUsername);

            bool isRequestExist = await _db.GroupJoinOrders
                .AnyAsync(g => g.GroupId == groupId
                            && g.AcceptorId == acceptorId
                            && g.SenderId == senderId);

            if (!isRequestExist)
            {
                throw new Exception("Request not found");
                return false;
            }
            else if (isRequestExist)
            {
                await _acceptStudentService.AcceptStudent(token, groupId, studentUsername);
                return true;
            }
            else             
            {
                throw new Exception("Unknown error");
            }
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
   
    [HttpPost("removeorder")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<ActionResult<bool>> RemoveOrder([FromBody] JsonElement body)
    {
        try
        {
            var authHeader = HttpContext.Request.Headers["Authorization"].ToString();
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            {
                return Unauthorized(new { Message = "Authorization header is missing or invalid" });
            }
            var token = authHeader.Substring("Bearer ".Length).Trim();
            string groupId = body.GetProperty("groupId").GetString();
            string username = body.GetProperty("username").GetString();
            string userId = await _userReader.GetIdByUsernameAsync(username);
            string removerId = await _userReader.GetIdAsync(token);

            await _db.GroupJoinOrders.Where(g => g.GroupId == groupId && g.AcceptorId == userId && g.SenderId == removerId).ExecuteDeleteAsync();
            await _db.GroupJoinOrders.Where(g => g.GroupId == groupId && g.AcceptorId == removerId && g.SenderId == userId).ExecuteDeleteAsync();
            await _db.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}