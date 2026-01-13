using FirebaseAdmin;
using FirebaseAdmin.Auth;
using Microsoft.EntityFrameworkCore;
using API.Infrastructure;
using API.Infrastructure.Persistence.SQLdatabase.SQLite.Context;
using Google.Cloud.Firestore;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore.V1;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

var firebaseProjectId = builder.Configuration["FirebaseProjectPrivateKey:project_id"];

if (FirebaseApp.DefaultInstance == null)
{
    FirebaseApp.Create(new AppOptions()
    {
        Credential = CredentialFactory.FromJson<GoogleCredential>(builder.Configuration["FirebaseProjectPrivateKey:private_key"]),
        ProjectId = firebaseProjectId
    });
}

var firestoreBuilder = new FirestoreClientBuilder
{
    Credential = CredentialFactory.FromJson<GoogleCredential>(builder.Configuration["FirebaseProjectPrivateKey:private_key"])
};

var firestoreClient = firestoreBuilder.Build();

builder.Services.AddSingleton(FirebaseAuth.DefaultInstance);
builder.Services.AddSingleton(_ => FirestoreDb.Create(firebaseProjectId, firestoreClient));

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
