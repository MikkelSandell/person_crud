using CrudApp.Backend.Models;
using CrudApp.Backend.Services;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Minio;

var builder = WebApplication.CreateBuilder(args);

// Mongo configuration
builder.Services.Configure<MongoSettings>(builder.Configuration.GetSection("Mongo"));
builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var settings = sp.GetRequiredService<IOptions<MongoSettings>>().Value;
    var connectionString = string.IsNullOrWhiteSpace(settings.ConnectionString)
        ? "mongodb://root:example@localhost:27017"
        : settings.ConnectionString;
    return new MongoClient(connectionString);
});
builder.Services.AddSingleton(sp =>
{
    var settings = sp.GetRequiredService<IOptions<MongoSettings>>().Value;
    var client = sp.GetRequiredService<IMongoClient>();
    var databaseName = string.IsNullOrWhiteSpace(settings.DatabaseName) ? "persons" : settings.DatabaseName;
    return client.GetDatabase(databaseName);
});
builder.Services.AddSingleton<IMongoCollection<Person>>(sp =>
{
    var settings = sp.GetRequiredService<IOptions<MongoSettings>>().Value;
    var database = sp.GetRequiredService<IMongoDatabase>();
    var collectionName = string.IsNullOrWhiteSpace(settings.CollectionName) ? "persons" : settings.CollectionName;
    return database.GetCollection<Person>(collectionName);
});

// MinIO configuration
builder.Services.AddSingleton<IMinioClient>(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var endpoint = configuration["MinIO:Endpoint"] ?? "localhost:9000";
    var accessKey = configuration["MinIO:AccessKey"] ?? "minioadmin";
    var secretKey = configuration["MinIO:SecretKey"] ?? "minioadmin";
    var useSSL = bool.Parse(configuration["MinIO:UseSSL"] ?? "false");

    return new MinioClient()
        .WithEndpoint(endpoint)
        .WithCredentials(accessKey, secretKey)
        .WithSSL(useSSL)
        .Build();
});
builder.Services.AddScoped<IFileService, FileService>();

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp",
        builderPolicy => builderPolicy
            .WithOrigins("http://localhost:3000")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials());
});

var app = builder.Build();

// Configure the HTTP request pipeline
app.UseCors("AllowReactApp");
app.UseHttpsRedirection();
app.MapControllers();

app.Run();
