using System;
using System.Linq;
using System.Threading.Tasks;
using LXGaming.Ticket.Server.Models;
using LXGaming.Ticket.Server.Models.Form;
using LXGaming.Ticket.Server.Security;
using LXGaming.Ticket.Server.Security.Authorization;
using LXGaming.Ticket.Server.Services.Event;
using LXGaming.Ticket.Server.Storage;
using LXGaming.Ticket.Server.Util;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LXGaming.Ticket.Server.Controllers {

    [ApiController]
    [Route("projects/{projectId}/issues/{issueId}/comments")]
    public class IssueCommentController : ControllerBase {

        private readonly StorageContext _context;
        private readonly EventService _eventService;

        public IssueCommentController(StorageContext context, EventService eventService) {
            _context = context;
            _eventService = eventService;
        }

        [HttpGet]
        [Scope(SecurityConstants.Scopes.IssueCommentRead, SecurityConstants.Scopes.IssueCommentWrite)]
        public async Task<IActionResult> GetAsync(string projectId, ulong issueId) {
            var comments = await _context.IssueComments
                .Include(model => model.Issue)
                .Where(model => model.IssueId == issueId)
                .Where(model => string.Equals(model.Issue.ProjectId, projectId, StringComparison.OrdinalIgnoreCase))
                .Select(model => model.Id)
                .ToArrayAsync();

            return Ok(comments);
        }

        [HttpGet("{id}")]
        [Scope(SecurityConstants.Scopes.IssueCommentRead, SecurityConstants.Scopes.IssueCommentWrite)]
        public async Task<IActionResult> GetAsync(string projectId, ulong issueId, ulong id) {
            var comment = await _context.IssueComments
                .Include(model => model.Issue)
                .Where(model => model.Id == id)
                .Where(model => model.IssueId == issueId)
                .SingleOrDefaultAsync();
            if (comment == null) {
                return NotFound();
            }

            if (!string.Equals(comment.Issue.ProjectId, projectId, StringComparison.OrdinalIgnoreCase)) {
                return RedirectPermanent($"/projects/{comment.Issue.ProjectId}/issues/{comment.Issue.Id}/comments");
            }

            return Ok(new {
                Id = comment.Id,
                Body = comment.Body,
                Issue = new {
                    Id = comment.IssueId
                },
                User = comment.GetUser(),
                CreatedAt = comment.CreatedAt,
                UpdatedAt = comment.UpdatedAt
            });
        }

        [HttpPost]
        [Scope(SecurityConstants.Scopes.IssueCommentWrite)]
        public async Task<IActionResult> CreateAsync(string projectId, ulong issueId, [FromBody] CreateIssueCommentForm form) {
            if (!ModelState.IsValid) {
                return BadRequest(ModelState);
            }

            var issue = await _context.Issues
                .Include(model => model.Comments)
                .SingleOrDefaultAsync(model => model.Id == issueId);
            if (issue == null) {
                return NotFound();
            }

            if (!string.Equals(issue.ProjectId, projectId, StringComparison.OrdinalIgnoreCase)) {
                return RedirectPermanent($"/projects/{issue.ProjectId}/issues/{issue.Id}/comments");
            }

            var user = await _context.Users
                .Include(model => model.Identifiers)
                .Include(model => model.Names)
                .SingleOrDefaultAsync(model => model.Id == form.UserId);
            if (user == null) {
                return BadRequest("Invalid User");
            }

            if (issue.UserId != user.Id) {
                issue.Read = false;
            }

            var comment = new IssueComment {
                Body = form.Body,
                User = user
            };
            issue.Comments.Add(comment);
            issue.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            await _eventService.OnIssueCommentCreatedAsync(comment);

            return Created($"/projects/{projectId}/issues/{issue.Id}/comments/{comment.Id}", new {
                Id = comment.Id,
                Body = comment.Body,
                Issue = new {
                    Id = comment.IssueId
                },
                User = comment.GetUser(),
                CreatedAt = comment.CreatedAt,
                UpdatedAt = comment.UpdatedAt
            });
        }
    }
}