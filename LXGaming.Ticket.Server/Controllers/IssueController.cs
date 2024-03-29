﻿using System;
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

namespace LXGaming.Ticket.Server.Controllers {

    [ApiController]
    [Route("projects/{projectId}/issues")]
    public class IssueController : ControllerBase {

        private readonly StorageContext _context;
        private readonly EventService _eventService;

        public IssueController(StorageContext context, EventService eventService) {
            _context = context;
            _eventService = eventService;
        }

        [HttpGet]
        [Scope(SecurityConstants.Scopes.IssueRead, SecurityConstants.Scopes.IssueWrite)]
        public async Task<IActionResult> GetAsync(string projectId, ulong? creator, IssueStatus? status, bool? read = null) {
            var issues = await _context.Issues
                .AsQueryable()
                .Where(model => string.Equals(model.ProjectId, projectId, StringComparison.OrdinalIgnoreCase))
                .Where(model => creator == null || model.UserId == creator)
                .Where(model => status == null || model.Status == status)
                .Where(model => read == null || model.Read == read)
                .Select(model => model.Id)
                .ToArrayAsync();

            return Ok(issues);
        }

        [HttpGet("{id}")]
        [Scope(SecurityConstants.Scopes.IssueRead, SecurityConstants.Scopes.IssueWrite)]
        public async Task<IActionResult> GetAsync(string projectId, ulong id) {
            var issue = await _context.Issues
                .Include(model => model.Comments)
                .ThenInclude(model => model.User)
                .ThenInclude(model => model.Names)
                .Include(model => model.User)
                .ThenInclude(model => model.Names)
                .SingleOrDefaultAsync(model => model.Id == id);
            if (issue == null) {
                return NotFound();
            }

            if (!string.Equals(issue.ProjectId, projectId, StringComparison.OrdinalIgnoreCase)) {
                return RedirectPermanent($"/projects/{issue.ProjectId}/issues/{issue.Id}");
            }

            return Ok(new {
                Id = issue.Id,
                Data = issue.Data,
                Body = issue.Body,
                Status = issue.Status,
                Read = issue.Read,
                Comments = issue.Comments.Select(comment => new {
                    Id = comment.Id,
                    Body = comment.Body,
                    User = comment.GetUser(),
                    CreatedAt = comment.GetCreatedAtUtc(),
                    UpdatedAt = comment.GetUpdatedAtUtc()
                }),
                Project = new {
                    Id = issue.ProjectId
                },
                User = issue.GetUser(),
                CreatedAt = issue.GetCreatedAtUtc(),
                UpdatedAt = issue.GetUpdatedAtUtc()
            });
        }

        [HttpPost]
        [Scope(SecurityConstants.Scopes.IssueWrite)]
        public async Task<IActionResult> CreateAsync(string projectId, [FromBody] CreateIssueForm form) {
            if (!ModelState.IsValid) {
                return BadRequest(ModelState);
            }

            if (!await _context.Projects.AsQueryable().AnyAsync(model => string.Equals(model.Id, projectId))) {
                return BadRequest("Invalid Project");
            }

            var user = await _context.Users
                .Include(model => model.Identifiers)
                .Include(model => model.Names)
                .SingleOrDefaultAsync(model => model.Id == form.UserId);
            if (user == null) {
                return BadRequest("Invalid User");
            }

            if (user.Banned) {
                return Forbid();
            }

            var issue = new Issue {
                ProjectId = projectId,
                Data = form.Data,
                Body = form.Body,
                Status = IssueStatus.Open,
                Read = true,
                User = user
            };
            _context.Issues.Add(issue);

            await _context.SaveChangesAsync();
            await _eventService.OnIssueCreatedAsync(issue);

            return Created($"/projects/{projectId}/issues/{issue.Id}", new {
                Id = issue.Id,
                Data = issue.Data,
                Body = issue.Body,
                Status = issue.Status,
                Read = issue.Read,
                User = issue.GetUser(),
                CreatedAt = issue.GetCreatedAtUtc(),
                UpdatedAt = issue.GetUpdatedAtUtc()
            });
        }

        [HttpPost("{id}")]
        [Scope(SecurityConstants.Scopes.IssueWrite)]
        public async Task<IActionResult> UpdateAsync(string projectId, ulong id, [FromBody] EditIssueForm form) {
            if (!ModelState.IsValid) {
                return BadRequest(ModelState);
            }

            var issue = await _context.Issues
                .Include(model => model.User)
                .Include(model => model.User.Identifiers)
                .Include(model => model.User.Names)
                .SingleOrDefaultAsync(model => model.Id == id);
            if (issue == null) {
                return NotFound();
            }

            if (!string.Equals(issue.ProjectId, projectId, StringComparison.OrdinalIgnoreCase)) {
                return RedirectPermanent($"/projects/{issue.ProjectId}/issues/{issue.Id}");
            }

            var eventArgs = new IssueUpdatedEventArgs(issue);

            var status = form.Status ?? issue.Status;
            if (issue.Status != status) {
                issue.Status = status;
            }

            var read = form.Read ?? issue.Read;
            if (issue.Read != read) {
                issue.Read = read;
            }

            await _context.SaveChangesAsync();
            await _eventService.OnIssueUpdatedAsync(eventArgs);
            return NoContent();
        }
    }
}