using Microsoft.AspNetCore.Mvc;
using MusicStreamingPrototype.API.Services;

namespace MusicStreamingPrototype.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HelloController : ControllerBase
    {
        private readonly IHelloService _helloService;

        public HelloController(IHelloService helloService)
        {
            _helloService = helloService;
        }

        [HttpGet]
        public IActionResult Get()
            => Ok(_helloService.GetMessage());
    }
}
