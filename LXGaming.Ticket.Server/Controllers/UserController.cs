using System;
using System.Linq;
using System.Threading.Tasks;
using LXGaming.Ticket.Server.Models;
using LXGaming.Ticket.Server.Models.Form;
using LXGaming.Ticket.Server.Security;
using LXGaming.Ticket.Server.Security.Authorization;
using LXGaming.Ticket.Server.Services.Event;
using LXGaming.Ticket.Server.Services.Event.Models;
using LXGaming.Ticket.Server.Storage;
using LXGaming.Ticket.Server.Util;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LXGaming.Ticket.Server.Controllers {

    [ApiController]
    [Route("users")]
    public class UserController : ControllerBase {

        private readonly StorageContext _context;
        private readonly ILogger<UserController> _logger;
        private readonly EventService _eventService;

        public UserController(StorageContext context, ILogger<UserController> logger, EventService eventService) {
            _context = context;
            _logger = logger;
            _eventService = eventService;
        }

        [HttpGet]
        [Scope(SecurityConstants.Scopes.UserRead, SecurityConstants.Scopes.UserWrite)]
        public async Task<IActionResult> GetAsync(string identifier) {
            if (!Toolbox.ParseIdentifier(identifier, out var key, out var value)) {
                return BadRequest();
            }

            var userId = await _context.UserIdentifiers
                .AsQueryable()
                .Where(model => string.Equals(model.IdentifierId, key, StringComparison.OrdinalIgnoreCase))
                .Where(model => string.Equals(model.Value, value, StringComparison.OrdinalIgnoreCase))
                .Select(model => model.UserId)
                .SingleOrDefaultAsync();

            if (userId == 0L) {
                return NotFound();
            }

            return Ok(new {
                Id = userId
            });
        }

        [HttpGet("{id}")]
        [Scope(SecurityConstants.Scopes.UserRead, SecurityConstants.Scopes.UserWrite)]
        public async Task<IActionResult> GetAsync(ulong id) {
            var user = await _context.Users
                .Include(model => model.Identifiers)
                .Include(model => model.Names)
                .SingleOrDefaultAsync(model => model.Id == id);
            if (user == null) {
                return NotFound();
            }

            return Ok(new {
                Banned = user.Banned,
                Identifier = user.Identifiers.ToDictionary(model => model.IdentifierId, model => model.Value),
                Names = user.Names.ToDictionary(model => model.ProjectId, model => model.Value),
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt
            });
        }

        [HttpPost]
        [Scope(SecurityConstants.Scopes.UserWrite)]
        public async Task<IActionResult> CreateAsync([FromBody] CreateUserForm form) {
            if (!ModelState.IsValid) {
                return BadRequest(ModelState);
            }

            var user = new User {
                Banned = false
            };
            _context.Users.Add(user);

            foreach (var (key, value) in form.Identifiers) {
                if (_context.UserIdentifiers.Local.Any(model => string.Equals(model.IdentifierId, key))) {
                    _logger.LogWarning("Duplicate identifier: {Identifier}", key);
                    continue;
                }

                if (!await _context.Identifiers.AsQueryable().AnyAsync(model => string.Equals(model.Id, key))) {
                    _logger.LogWarning("Unsupported identifier: {Identifier}", key);
                    continue;
                }

                if (await _context.UserIdentifiers.AsQueryable().AnyAsync(model => string.Equals(model.IdentifierId, key) && string.Equals(model.Value, value))) {
                    _logger.LogWarning("Duplicate identifier: {Identifier}", key);
                    continue;
                }

                user.Identifiers.Add(new UserIdentifier {
                    IdentifierId = key,
                    Value = value,
                    User = user
                });
            }

            if (_context.UserIdentifiers.Local.Count == 0) {
                return BadRequest("Missing identifiers");
            }

            foreach (var (key, value) in form.Names) {
                if (_context.UserNames.Local.Any(model => string.Equals(model.ProjectId, key))) {
                    _logger.LogWarning("Duplicate name: {Project}", key);
                    continue;
                }

                if (!await _context.Projects.AsQueryable().AnyAsync(model => string.Equals(model.Id, key))) {
                    _logger.LogWarning("Unsupported project: {Project}", key);
                    continue;
                }

                _context.UserNames.Add(new UserName {
                    ProjectId = key,
                    Value = value,
                    User = user
                });
            }

            if (_context.UserNames.Local.Count == 0) {
                return BadRequest("Missing names");
            }

            await _context.SaveChangesAsync();

            return Created($"/users/{user.Id}", new {
                Id = user.Id
            });
        }

        [HttpPost("{id}")]
        [Scope(SecurityConstants.Scopes.UserWrite)]
        public async Task<IActionResult> UpdateAsync(ulong id, [FromBody] EditUserForm form) {
            if (!ModelState.IsValid) {
                return BadRequest(ModelState);
            }

            var user = await _context.Users
                .Include(model => model.Identifiers)
                .Include(model => model.Names)
                .SingleOrDefaultAsync(model => model.Id == id);
            if (user == null) {
                return NotFound();
            }

            var eventArgs = new UserUpdatedEventArgs(user);

            var banned = form.Banned ?? user.Banned;
            if (user.Banned != banned) {
                user.Banned = banned;
            }

            if (form.Identifiers != null) {
                foreach (var (key, value) in form.Identifiers) {
                    if (user.Identifiers.Any(model => string.Equals(model.IdentifierId, key))) {
                        _logger.LogWarning("Duplicate identifier: {Identifier}", key);
                        continue;
                    }

                    if (!await _context.Identifiers.AsQueryable().AnyAsync(model => string.Equals(model.Id, key))) {
                        _logger.LogWarning("Unsupported identifier: {Identifier}", key);
                        continue;
                    }

                    _context.UserIdentifiers.Add(new UserIdentifier {
                        IdentifierId = key,
                        Value = value,
                        User = user
                    });
                }
            }

            if (form.Names != null) {
                foreach (var (key, value) in form.Names) {
                    var existingProject = user.Names.SingleOrDefault(model => string.Equals(model.ProjectId, key));
                    if (existingProject != null) {
                        existingProject.Value = value;
                        continue;
                    }

                    if (!await _context.Projects.AsQueryable().AnyAsync(model => string.Equals(model.Id, key))) {
                        _logger.LogWarning("Unsupported identifier: {Identifier}", key);
                        continue;
                    }

                    _context.UserNames.Add(new UserName {
                        ProjectId = key,
                        Value = value,
                        User = user
                    });
                }
            }

            await _context.SaveChangesAsync();
            await _eventService.OnUserUpdatedAsync(eventArgs);
            return NoContent();
        }
    }
}