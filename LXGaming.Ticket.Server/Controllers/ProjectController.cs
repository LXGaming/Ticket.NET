using System.Threading.Tasks;
using LXGaming.Ticket.Server.Security;
using LXGaming.Ticket.Server.Security.Authorization;
using LXGaming.Ticket.Server.Storage;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LXGaming.Ticket.Server.Controllers {

    [ApiController]
    [Route("projects")]
    public class ProjectController : ControllerBase {

        private readonly StorageContext _context;

        public ProjectController(StorageContext context) {
            _context = context;
        }

        [HttpGet]
        [Scope(SecurityConstants.Scopes.ProjectRead)]
        public async Task<IActionResult> GetAsync() {
            return Ok(await _context.Projects.ToDictionaryAsync(model => model.Id, model => model.Name));
        }
    }
}