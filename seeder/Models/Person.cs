using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Seeder.Models
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

        [BsonElement("profilePicture")]
        public string ProfilePicture { get; set; } = string.Empty;

        [BsonElement("starSign")]
        public string StarSign { get; set; } = string.Empty;

        [BsonElement("friendIds")]
        public List<string> FriendIds { get; set; } = new();
    }
}
