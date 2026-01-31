using FirebaseAdmin;
using FirebaseAdmin.Auth;
using Microsoft.EntityFrameworkCore;
using API.Infrastructure;
using API.Infrastructure.Database;
using API.Infrastructure.DI;
using Google.Cloud.Firestore;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore.V1;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using API.Application.Interfaces.Users.IGroup;
using API.Application.Interfaces.Users.IStudent;
using API.Application.Interfaces.Users.ITeacher;
using API.Application.Interfaces.Users.IUser;
using System.Text.Json.Serialization;
using API.Infrastructure.Implementations.UserRepository;
using API.Infrastructure.Implementations.StudentRepository;
using API.Infrastructure.Implementations.TeacherRepository;
using API.Infrastructure.Implementations.GroupRepository;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add SQL Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

//Get service accounts
var json = JsonConvert.SerializeObject(
    builder.Configuration.GetSection("FirebaseProjectPrivateKey").GetChildren()
    .ToDictionary(x => x.Key, x => x.Value)
);

//Add Firebase
var firebaseProjectId = builder.Configuration["FirebaseProjectPrivateKey:project_id"];
if (FirebaseApp.DefaultInstance == null)
{
    FirebaseApp.Create(new AppOptions()
    {
        Credential = GoogleCredential.FromJson(json),
        ProjectId = firebaseProjectId
    });
}

//Add Firestore
var firestoreBuilder = new FirestoreClientBuilder
{
    Credential = GoogleCredential.FromJson(json)
};
var firestoreClient = firestoreBuilder.Build();

//Singletons
builder.Services.AddSingleton(FirebaseAuth.DefaultInstance);
builder.Services.AddSingleton(_ => FirestoreDb.Create(firebaseProjectId, firestoreClient));

builder.Services.AddControllers();
builder.Services.AddControllers()
    .AddJsonOptions(o =>
        o.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)));

builder.Services.AddInfrastructure();

builder.Services.AddScoped<IUserReader, UserRepo>();
builder.Services.AddScoped<IUserWriter, UserRepo>();

builder.Services.AddScoped<IStudentReader, StudentRepo>();
builder.Services.AddScoped<IStudentWriter, StudentRepo>();
builder.Services.AddScoped<IStudentCalculator, StudentCalculator>();


builder.Services.AddScoped<ITeacherReader, TeacherRepo>();
builder.Services.AddScoped<ITeacherWriter, TeacherRepo>();

builder.Services.AddScoped<IGroupReader, GroupRepo>();
builder.Services.AddScoped<IGroupWriter, GroupRepo>();


// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddCors(o => o.AddPolicy("AllowFrontend", p => 
    p.WithOrigins("http://127.0.0.1:5500", "http://localhost:5500")
     .AllowAnyHeader()
     .AllowAnyMethod()
     .AllowCredentials()));  // Important for Authorization headers

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = "https://securetoken.google.com/" + firebaseProjectId;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = $"https://securetoken.google.com/{firebaseProjectId}",
            ValidateAudience = true,
            ValidAudience = firebaseProjectId,
            ValidateLifetime = true
        };
    });
builder.Services.AddAuthorization();
builder.Services.AddEndpointsApiExplorer();
//-------------------------------
var app = builder.Build();
//-------------------------------
// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
    //app.MapOpenApi();
//}

app.UseDeveloperExceptionPage();

app.UseHttpsRedirection();

app.UseCors("AllowFrontend");

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
