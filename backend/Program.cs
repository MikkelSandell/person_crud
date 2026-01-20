using CrudApp.Backend.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

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
