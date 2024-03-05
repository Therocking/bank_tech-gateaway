using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Bank_tech_gateaway.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        [HttpGet("hello")]
        public IActionResult ShowHello()
        {
            return Ok();
        }
    }
}
