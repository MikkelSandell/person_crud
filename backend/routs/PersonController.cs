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

            string profilePictureUrl = string.Empty;
            
            // Handle profile picture if it's a base64 string
            if (!string.IsNullOrWhiteSpace(request.ProfilePicture) && request.ProfilePicture.StartsWith("data:image"))
            {
                try
                {
                    var base64Data = request.ProfilePicture.Split(',')[1];
                    var imageBytes = Convert.FromBase64String(base64Data);
                    var contentType = request.ProfilePicture.Split(';')[0].Split(':')[1];
                    var extension = contentType.Split('/')[1];
                    var fileName = $"{Guid.NewGuid()}.{extension}";

                    using var stream = new MemoryStream(imageBytes);
                    profilePictureUrl = await _fileService.UploadFileAsync(stream, fileName, contentType);
                }
                catch (Exception ex)
                {
                    return BadRequest($"Error uploading profile picture: {ex.Message}");
                }
            }
            else
            {
                profilePictureUrl = request.ProfilePicture ?? string.Empty;
            }

            var person = new Person
            {
                Username = request.Username.Trim(),
                Cpr = request.Cpr.Trim(),
                ProfilePicture = profilePictureUrl,
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

            string profilePictureUrl = request.ProfilePicture ?? string.Empty;

            // Handle profile picture update
            if (!string.IsNullOrWhiteSpace(request.ProfilePicture) && request.ProfilePicture.StartsWith("data:image"))
            {
                try
                {
                    // Delete old picture if it exists and is from MinIO
                    if (!string.IsNullOrWhiteSpace(existingPerson.ProfilePicture) && 
                        existingPerson.ProfilePicture.Contains("localhost:9000"))
                    {
                        var oldFileName = ExtractFileNameFromUrl(existingPerson.ProfilePicture);
                        if (!string.IsNullOrWhiteSpace(oldFileName))
                        {
                            await _fileService.DeleteFileAsync(oldFileName);
                        }
                    }

                    // Upload new picture
                    var base64Data = request.ProfilePicture.Split(',')[1];
                    var imageBytes = Convert.FromBase64String(base64Data);
                    var contentType = request.ProfilePicture.Split(';')[0].Split(':')[1];
                    var extension = contentType.Split('/')[1];
                    var fileName = $"{Guid.NewGuid()}.{extension}";

                    using var stream = new MemoryStream(imageBytes);
                    profilePictureUrl = await _fileService.UploadFileAsync(stream, fileName, contentType);
                }
                catch (Exception ex)
                {
                    return BadRequest($"Error uploading profile picture: {ex.Message}");
                }
            }

            var update = Builders<Person>.Update
                .Set(p => p.Username, request.Username?.Trim() ?? string.Empty)
                .Set(p => p.Cpr, request.Cpr?.Trim() ?? string.Empty)
                .Set(p => p.ProfilePicture, profilePictureUrl)
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

            // Delete profile picture from MinIO if it exists
            if (!string.IsNullOrWhiteSpace(person.ProfilePicture) && 
                person.ProfilePicture.Contains("localhost:9000"))
            {
                var fileName = ExtractFileNameFromUrl(person.ProfilePicture);
                if (!string.IsNullOrWhiteSpace(fileName))
                {
                    await _fileService.DeleteFileAsync(fileName);
                }
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
