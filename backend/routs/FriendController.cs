using CrudApp.Backend.Models;
using CrudApp.Backend.Util;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;

namespace CrudApp.Backend.Routes
{
    [ApiController]
    [Route("api/[controller]")]
    public class FriendController : ControllerBase
    {
        private readonly IMongoCollection<Person> _persons;

        public FriendController(IMongoCollection<Person> persons)
        {
            _persons = persons;
        }

        [HttpPost("{id}/add/{friendId}")]
        public async Task<ActionResult<PersonResponse>> AddFriend(string id, string friendId)
        {
            if (!ObjectId.TryParse(id, out _) || !ObjectId.TryParse(friendId, out _))
            {
                return BadRequest("Invalid id format.");
            }

            if (id == friendId)
            {
                return BadRequest("Cannot add yourself as a friend.");
            }

            // Add friendId to person's friend list
            var update = Builders<Person>.Update.AddToSet(p => p.FriendIds, friendId);
            var result = await _persons.FindOneAndUpdateAsync<Person>(
                p => p.Id == id,
                update,
                new FindOneAndUpdateOptions<Person>
                {
                    ReturnDocument = ReturnDocument.After
                },
                cancellationToken: default);

            if (result is null)
            {
                return NotFound();
            }

            // Also add person's id to friend's friend list (mutual friendship)
            var friendUpdate = Builders<Person>.Update.AddToSet(p => p.FriendIds, id);
            await _persons.UpdateOneAsync(
                p => p.Id == friendId,
                friendUpdate);

            return Ok(ToResponse(result));
        }

        [HttpDelete("{id}/remove/{friendId}")]
        public async Task<ActionResult<PersonResponse>> RemoveFriend(string id, string friendId)
        {
            if (!ObjectId.TryParse(id, out _) || !ObjectId.TryParse(friendId, out _))
            {
                return BadRequest("Invalid id format.");
            }

            // Remove friendId from person's friend list
            var update = Builders<Person>.Update.Pull(p => p.FriendIds, friendId);
            var result = await _persons.FindOneAndUpdateAsync<Person>(
                p => p.Id == id,
                update,
                new FindOneAndUpdateOptions<Person>
                {
                    ReturnDocument = ReturnDocument.After
                },
                cancellationToken: default);

            if (result is null)
            {
                return NotFound();
            }

            // Also remove person's id from friend's friend list (mutual friendship removal)
            var friendUpdate = Builders<Person>.Update.Pull(p => p.FriendIds, id);
            await _persons.UpdateOneAsync(
                p => p.Id == friendId,
                friendUpdate);

            return Ok(ToResponse(result));
        }

        private static PersonResponse ToResponse(Person p)
        {
            PersonUtil.TryParseBirthDateFromCpr(p.Cpr, out var birthDate);
            var age = PersonUtil.CalculateAgeFromCpr(p.Cpr);
            var starSign = birthDate == default ? string.Empty : PersonUtil.GetStarSign(birthDate) ?? string.Empty;

            return new PersonResponse
            {
                Id = p.Id ?? string.Empty,
                Username = p.Username,
                Cpr = p.Cpr,
                ProfilePicture = p.ProfilePicture,
                Age = age,
                StarSign = starSign,
                FriendIds = p.FriendIds
            };
        }
    }
}
