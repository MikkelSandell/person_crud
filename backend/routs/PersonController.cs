using CrudApp.Backend.Models;
using CrudApp.Backend.Util;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;

namespace CrudApp.Backend.Routes
{
    [ApiController]
    [Route("api/[controller]")]
    public class PersonController : ControllerBase
    {
        private readonly IMongoCollection<Person> _persons;

        public PersonController(IMongoCollection<Person> persons)
        {
            _persons = persons;
        }

        [HttpGet]
        public async Task<ActionResult<dynamic>> GetAll([FromQuery] int skip = 0, [FromQuery] int pageSize = 5, [FromQuery] string sortBy = "username", [FromQuery] string sortOrder = "asc")
        {
            var total = await _persons.CountDocumentsAsync(_ => true);
            
            var findOptions = PersonSorting.GetFindOptions();
            var filter = Builders<Person>.Filter.Empty;
            var sortDef = PersonSorting.GetSortDefinition(sortBy, sortOrder);

            var people = await _persons
                .Find(filter, findOptions)
                .Sort(sortDef)
                .Skip(skip)
                .Limit(pageSize)
                .ToListAsync();
            
            return Ok(new
            {
                items = people.Select(ToResponse),
                total = total,
                skip = skip,
                pageSize = pageSize,
                sortBy = sortBy,
                sortOrder = sortOrder
            });
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PersonResponse>> GetById(string id)
        {
            if (!ObjectId.TryParse(id, out _))
            {
                return BadRequest("Invalid id format.");
            }

            var person = await _persons.Find(p => p.Id == id).FirstOrDefaultAsync();
            if (person is null)
            {
                return NotFound();
            }

            return Ok(ToResponse(person));
        }

        [HttpPost]
        public async Task<ActionResult<PersonResponse>> Create([FromBody] PersonRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Cpr))
            {
                return BadRequest("Username and CPR are required.");
            }

            PersonUtil.TryParseBirthDateFromCpr(request.Cpr, out var birthDate);
            var starSign = birthDate == default ? string.Empty : PersonUtil.GetStarSign(birthDate) ?? string.Empty;

            var person = new Person
            {
                Username = request.Username.Trim(),
                Cpr = request.Cpr.Trim(),
                ProfilePicture = request.ProfilePicture ?? string.Empty,
                StarSign = starSign
            };

            await _persons.InsertOneAsync(person);

            var response = ToResponse(person);
            return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<PersonResponse>> Update(string id, [FromBody] PersonRequest request)
        {
            if (!ObjectId.TryParse(id, out _))
            {
                return BadRequest("Invalid id format.");
            }

            PersonUtil.TryParseBirthDateFromCpr(request.Cpr, out var birthDate);
            var starSign = birthDate == default ? string.Empty : PersonUtil.GetStarSign(birthDate) ?? string.Empty;

            var update = Builders<Person>.Update
                .Set(p => p.Username, request.Username?.Trim() ?? string.Empty)
                .Set(p => p.Cpr, request.Cpr?.Trim() ?? string.Empty)
                .Set(p => p.ProfilePicture, request.ProfilePicture ?? string.Empty)
                .Set(p => p.StarSign, starSign);

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

            return Ok(ToResponse(result));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            if (!ObjectId.TryParse(id, out _))
            {
                return BadRequest("Invalid id format.");
            }

            var deleteResult = await _persons.DeleteOneAsync(p => p.Id == id);
            if (deleteResult.DeletedCount == 0)
            {
                return NotFound();
            }

            return NoContent();
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
