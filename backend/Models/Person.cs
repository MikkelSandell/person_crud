using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CrudApp.Backend.Models
{
    public class Person
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("username")]
        public string Username { get; set; } = string.Empty;

        [BsonElement("cpr")]
        public string Cpr { get; set; } = string.Empty;

        // Expect base64 or URL for the profile picture
        [BsonElement("profilePicture")]
        public string ProfilePicture { get; set; } = string.Empty;

        [BsonElement("starSign")]
        public string StarSign { get; set; } = string.Empty;

        [BsonElement("friendIds")]
        public List<string> FriendIds { get; set; } = new();
    }

    public class PersonRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Cpr { get; set; } = string.Empty;
        public string ProfilePicture { get; set; } = string.Empty;
    }

    public class PersonResponse
    {
        public string Id { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Cpr { get; set; } = string.Empty;
        public string ProfilePicture { get; set; } = string.Empty;
        public int? Age { get; set; }
        public string StarSign { get; set; } = string.Empty;
        public List<string> FriendIds { get; set; } = new();
    }

    public class MongoSettings
    {
        public string? ConnectionString { get; set; }
        public string? DatabaseName { get; set; }
        public string? CollectionName { get; set; }
    }
}
