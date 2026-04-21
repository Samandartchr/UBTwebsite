using FirebaseAdmin;
using FirebaseAdmin.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
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
    options.UseNpgsql(
    builder.Configuration.GetConnectionString("DefaultConnection")
));

// 1. Define the path
var jsonPath = "secret.json";

// 2. Extract the Project ID manually from the JSON
var jsonContent = File.ReadAllText(jsonPath);
using var jsonDoc = JsonDocument.Parse(jsonContent);
var firebaseProjectId = jsonDoc.RootElement.GetProperty("project_id").GetString();

// 3. Create the credential object
var cred = GoogleCredential.FromFile(jsonPath);

// 4. Initialize Firebase
if (FirebaseApp.DefaultInstance == null)
{
    FirebaseApp.Create(new AppOptions()
    {
        Credential = cred,
        ProjectId = firebaseProjectId 
    });
}

// 5. Initialize Firestore
var firestoreBuilder = new FirestoreClientBuilder { Credential = cred };
var firestoreClient = firestoreBuilder.Build();

builder.Services.AddSingleton(FirebaseAuth.DefaultInstance);
// Use the extracted string here
builder.Services.AddSingleton(_ => FirestoreDb.Create(firebaseProjectId, firestoreClient));
//Singletons
builder.Services.AddSingleton(FirebaseAuth.DefaultInstance);
builder.Services.AddSingleton(_ => FirestoreDb.Create(firebaseProjectId, firestoreClient));
builder.Services.AddControllers();
/*builder.Services.AddControllers()
    .AddJsonOptions(o =>
        o.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)));*/

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
//builder.Services.AddOpenApi();

builder.Services.AddCors(o => o.AddPolicy("AllowFrontend", p => 
    p.AllowAnyOrigin()
     .AllowAnyHeader()
     .AllowAnyMethod()));  // Important for Authorization headers

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

var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
//-------------------------------
var app = builder.Build();
//-------------------------------
// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
    //app.MapOpenApi();
//}

app.UseDeveloperExceptionPage();

//app.UseHttpsRedirection();

app.UseCors("AllowFrontend");

app.UseAuthentication();

app.UseAuthorization();
app.MapGet("/ping", () => "ok");
app.MapControllers();


app.Run();
