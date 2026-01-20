using MongoDB.Driver;
using Seeder.Models;
using Seeder.Services;

var mongoUrl = Environment.GetEnvironmentVariable("MONGO_CONNECTION_STRING") ?? "mongodb://root:example@mongo:27017";
var databaseName = Environment.GetEnvironmentVariable("MONGO_DATABASE") ?? "persons";
var collectionName = Environment.GetEnvironmentVariable("MONGO_COLLECTION") ?? "persons";

Console.WriteLine("🌱 Starting database seeder...");
Console.WriteLine($"Connecting to: {mongoUrl.Replace("root:example", "root:****")}");

try
{
    var client = new MongoClient(mongoUrl);
    var database = client.GetDatabase(databaseName);
    var collection = database.GetCollection<Person>(collectionName);

    await DatabaseSeeder.SeedDatabaseAsync(collection);
    Console.WriteLine("✅ Seeding completed successfully");
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Seeding failed: {ex.Message}");
    Environment.Exit(1);
}
