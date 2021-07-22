using Microsoft.AspNetCore.Mvc;

namespace LXGaming.Ticket.Server.Controllers {

    [ApiController]
    [Route("")]
    public class HomeController : ControllerBase {

        [HttpGet]
        public IActionResult Get() {
            return Ok(new {
                Application = "Ticket API"
            });
        }
    }
}