using MongoDB.Driver;
using Seeder.Models;
using Seeder.Utils;

namespace Seeder.Services
{
    public static class DatabaseSeeder
    {
        public static async Task SeedDatabaseAsync(IMongoCollection<Person> personsCollection)
        {
            // Clear existing data
            await personsCollection.DeleteManyAsync(_ => true);

            // Create test persons with valid CPR dates
            var testPersons = new List<Person>
            {
                new()
                {
                    Username = "Alice",
                    Cpr = "100195-1234",
                    ProfilePicture = string.Empty,
                    StarSign = GetStarSign("100195")
                },
                new()
                {
                    Username = "Bob",
                    Cpr = "150688-5678",
                    ProfilePicture = string.Empty,
                    StarSign = GetStarSign("150688")
                },
                new()
                {
                    Username = "Charlie",
                    Cpr = "200392-9012",
                    ProfilePicture = string.Empty,
                    StarSign = GetStarSign("200392")
                },
                new()
                {
                    Username = "Diana",
                    Cpr = "101075-3456",
                    ProfilePicture = string.Empty,
                    StarSign = GetStarSign("101075")
                },
                new()
                {
                    Username = "Eve",
                    Cpr = "051200-7890",
                    ProfilePicture = string.Empty,
                    StarSign = GetStarSign("051200")
                },
                new()
                {
                    Username = "Frank",
                    Cpr = "140585-2345",
                    ProfilePicture = string.Empty,
                    StarSign = GetStarSign("140585")
                },
                new()
                {
                    Username = "Grace",
                    Cpr = "200770-6789",
                    ProfilePicture = string.Empty,
                    StarSign = GetStarSign("200770")
                }
            };

            await personsCollection.InsertManyAsync(testPersons);
            Console.WriteLine("✓ Database seeded with 7 test persons");
        }

        private static string GetStarSign(string ddmmyy)
        {
            int day = int.Parse(ddmmyy.Substring(0, 2));
            int month = int.Parse(ddmmyy.Substring(2, 2));
            int year = int.Parse(ddmmyy.Substring(4, 2));

            var fullYear = year <= 29 ? 2000 + year : 1900 + year;
            var birthDate = new DateTime(fullYear, month, day);

            return PersonUtil.GetStarSign(birthDate) ?? string.Empty;
        }
    }
}
