using Microsoft.AspNetCore.Mvc;

namespace CrudApp.Backend.Routes
{
    [ApiController]
    [Route("api/[controller]")]
    public class GreetingsController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetGreeting()
        {
            return Ok(new { message = "Hello! Welcome to the Greetings API" });
        }

        [HttpGet("{name}")]
        public IActionResult GetGreetingByName(string name)
        {
            var greetings = new[]
            {
                $"Hello, {name}! Welcome to the Greetings API",
                $"Hi {name}, glad you're here!",
                $"Hey {name}! Hope you're having a great day",
                $"Greetings {name}, thanks for stopping by",
                $"Welcome {name}! Enjoy the API"
            };

            var message = greetings[Random.Shared.Next(greetings.Length)];
            return Ok(new { message });
        }
    }
}
