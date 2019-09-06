using Microsoft.AspNetCore.Mvc;

namespace Application.Controllers
{
    [Route("api/[controller]")]
    public class HealthController : Controller
    {
        // GET api/health/ping
        [Produces("application/json")]
        [HttpGet("ping")]
        public ActionResult Ping()
        {
            return Ok(new { Message = "pong!" });
        }
    }
}