using CrudApp.Backend.Models;
using CrudApp.Backend.Services;
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
        private readonly IFileService _fileService;

        public PersonController(IMongoCollection<Person> persons, IFileService fileService)
        {
            _persons = persons;
            _fileService = fileService;
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

            string profilePictureFileName = string.Empty;
            
            // Handle profile picture if it's a base64 string
            if (!string.IsNullOrWhiteSpace(request.ProfilePicture) && request.ProfilePicture.StartsWith("data:image"))
            {
                try
                {
                    profilePictureFileName = await _fileService.UploadBase64ImageAsync(request.ProfilePicture);
                }
                catch (Exception ex)
                {
                    return BadRequest($"Error uploading profile picture: {ex.Message}");
                }
            }
            else
            {
                profilePictureFileName = request.ProfilePicture ?? string.Empty;
            }

            var person = new Person
            {
                Username = request.Username.Trim(),
                Cpr = request.Cpr.Trim(),
                ProfilePicture = profilePictureFileName,
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

            var existingPerson = await _persons.Find(p => p.Id == id).FirstOrDefaultAsync();
            if (existingPerson is null)
            {
                return NotFound();
            }

            PersonUtil.TryParseBirthDateFromCpr(request.Cpr, out var birthDate);
            var starSign = birthDate == default ? string.Empty : PersonUtil.GetStarSign(birthDate) ?? string.Empty;

            string profilePictureFileName = request.ProfilePicture ?? string.Empty;

            // Handle profile picture update
            if (!string.IsNullOrWhiteSpace(request.ProfilePicture) && request.ProfilePicture.StartsWith("data:image"))
            {
                try
                {
                    // Delete old picture if it exists (it's a filename now, not a URL)
                    if (!string.IsNullOrWhiteSpace(existingPerson.ProfilePicture) && 
                        !existingPerson.ProfilePicture.StartsWith("http"))
                    {
                        await _fileService.DeleteFileAsync(existingPerson.ProfilePicture);
                    }

                    // Upload new picture
                    profilePictureFileName = await _fileService.UploadBase64ImageAsync(request.ProfilePicture);
                }
                catch (Exception ex)
                {
                    return BadRequest($"Error uploading profile picture: {ex.Message}");
                }
            }

            var update = Builders<Person>.Update
                .Set(p => p.Username, request.Username?.Trim() ?? string.Empty)
                .Set(p => p.Cpr, request.Cpr?.Trim() ?? string.Empty)
                .Set(p => p.ProfilePicture, profilePictureFileName)
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

            var person = await _persons.Find(p => p.Id == id).FirstOrDefaultAsync();
            if (person is null)
            {
                return NotFound();
            }

            // Remove this person from all other friend lists to avoid dangling references
            await _persons.UpdateManyAsync(
                Builders<Person>.Filter.AnyEq(p => p.FriendIds, id),
                Builders<Person>.Update.Pull(p => p.FriendIds, id));

            // Delete profile picture from MinIO if it exists (it's a filename now, not a URL)
            if (!string.IsNullOrWhiteSpace(person.ProfilePicture) && 
                !person.ProfilePicture.StartsWith("http"))
            {
                await _fileService.DeleteFileAsync(person.ProfilePicture);
            }

            var deleteResult = await _persons.DeleteOneAsync(p => p.Id == id);
            if (deleteResult.DeletedCount == 0)
            {
                return NotFound();
            }

            return NoContent();
        }

        private static string ExtractFileNameFromUrl(string url)
        {
            try
            {
                var uri = new Uri(url);
                var path = uri.AbsolutePath;
                var parts = path.Split('/');
                return parts.Length >= 3 ? parts[2] : string.Empty; // /bucket-name/filename
            }
            catch
            {
                return string.Empty;
            }
        }

        private PersonResponse ToResponse(Person p)
        {
            PersonUtil.TryParseBirthDateFromCpr(p.Cpr, out var birthDate);
            var age = PersonUtil.CalculateAgeFromCpr(p.Cpr);
            var starSign = birthDate == default ? string.Empty : PersonUtil.GetStarSign(birthDate) ?? string.Empty;

            // Generate fresh presigned URL if ProfilePicture is a filename (not a URL)
            string profilePictureUrl = p.ProfilePicture;
            if (!string.IsNullOrWhiteSpace(p.ProfilePicture) && !p.ProfilePicture.StartsWith("http"))
            {
                try
                {
                    profilePictureUrl = _fileService.GetFileUrlAsync(p.ProfilePicture).Result;
                }
                catch
                {
                    profilePictureUrl = string.Empty;
                }
            }

            return new PersonResponse
            {
                Id = p.Id ?? string.Empty,
                Username = p.Username,
                Cpr = p.Cpr,
                ProfilePicture = profilePictureUrl,
                Age = age,
                StarSign = starSign,
                FriendIds = p.FriendIds
            };
        }
    }
}
