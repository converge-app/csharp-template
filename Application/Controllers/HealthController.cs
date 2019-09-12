using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Application.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    public class HealthController : Controller
    {
        private readonly IHttpClientFactory clientFactory;

        public HealthController(IHttpClientFactory clientFactory)
        {
            this.clientFactory = clientFactory;
        }

        // GET api/health/ping
        [Produces("application/json")]
        [HttpGet("ping")]
        public async Task<ActionResult> PingAsync()
        {
//            var client = clientFactory.CreateClient ();
//
//            var request = new HttpRequestMessage (HttpMethod.Get, "http://localhost:5000/api/health/ping");
//
//            var response = await client.SendAsync (request);
//
//            if (response.IsSuccessStatusCode) {
            return Ok(new {Message = "pong!"});
//            } else {
//                return BadRequest ("Cannot connect to localhost:5000");
//            }
        }
    }
}