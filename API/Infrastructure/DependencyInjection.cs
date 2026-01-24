
using Microsoft.Extensions.DependencyInjection;

using API.Application.UseCases.Users.Register;

using API.Application.UseCases.Students.AcceptInvite;
using API.Application.UseCases.Students.JoinGroup;
using API.Application.UseCases.Students.GetTest;
using API.Application.UseCases.Students.GetTestResults;
using API.Application.UseCases.Students.PassTest;

using API.Application.UseCases.Teachers.AcceptStudent;
using API.Application.UseCases.Teachers.CreateGroup;
using API.Application.UseCases.Teachers.InviteStudent;
using API.Application.UseCases.Teachers.RemoveStudent;

using API.Application.UseCases.Groups.GetGroupInfo;
using API.Application.UseCases.Groups.GetGroupResults;
using API.Application.UseCases.Groups.GetStudents;
using Google.Protobuf.WellKnownTypes;

namespace API.Infrastructure.DI;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<RegisterService>();
        services.AddScoped<API.Application.UseCases.Users.ChangeSettings.ChangeSettingsService>();
        services.AddScoped<AcceptInviteService>();
        services.AddScoped<JoinGroupService>();
        services.AddScoped<API.Application.UseCases.Students.GetGroups.GetGroupsService>();
        services.AddScoped<GetTestService>();
        services.AddScoped<GetTestResultsService>();
        services.AddScoped<PassTestService>();
        services.AddScoped<AcceptStudentService>();
        services.AddScoped<CreateGroupService>();
        services.AddScoped<API.Application.UseCases.Teachers.GetGroups.GetGroupsService>();
        services.AddScoped<InviteStudentService>();
        services.AddScoped<RemoveStudentService>();
        services.AddScoped<API.Application.UseCases.Groups.ChangeSettings.ChangeSettingsService>();
        services.AddScoped<GetGroupInfoService>();
        services.AddScoped<GetGroupResultsService>();
        services.AddScoped<GetStudentsService>();

        return services;
    }
}