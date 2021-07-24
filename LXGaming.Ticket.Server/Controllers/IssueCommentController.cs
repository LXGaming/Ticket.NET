﻿using System;
using System.Linq;
using System.Threading.Tasks;
using LXGaming.Ticket.Server.Models;
using LXGaming.Ticket.Server.Models.Form;
using LXGaming.Ticket.Server.Security;
using LXGaming.Ticket.Server.Security.Authorization;
using LXGaming.Ticket.Server.Storage;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LXGaming.Ticket.Server.Controllers {

    [ApiController]
    [Route("projects/{projectId}/issues/{issueId}/comments")]
    public class IssueCommentController : ControllerBase {

        private readonly StorageContext _context;

        public IssueCommentController(StorageContext context) {
            _context = context;
        }

        [HttpGet("{id}")]
        [Scope(SecurityConstants.Scopes.IssueCommentRead, SecurityConstants.Scopes.IssueCommentWrite)]
        public async Task<IActionResult> GetAsync(string projectId, ulong issueId, ulong id) {
            var issueComment = await _context.IssueComments
                .Include(model => model.Issue)
                .Where(model => model.Id == id)
                .Where(model => model.IssueId == issueId)
                .SingleOrDefaultAsync();
            if (issueComment == null) {
                return NotFound();
            }

            if (!string.Equals(issueComment.Issue.ProjectId, projectId, StringComparison.OrdinalIgnoreCase)) {
                return RedirectPermanent($"/projects/{issueComment.Issue.ProjectId}/issues/{issueComment.Issue.Id}/comments");
            }

            return Ok(new {
                IssueId = issueComment.IssueId,
                UserId = issueComment.UserId,
                Body = issueComment.Body,
                CreatedAt = issueComment.CreatedAt,
                UpdatedAt = issueComment.UpdatedAt
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
                .Include(model => model.Projects)
                .SingleOrDefaultAsync(model => model.Id == form.UserId);
            if (user == null) {
                return BadRequest("Invalid User");
            }

            var comment = new IssueComment {
                Body = form.Body,
                User = user
            };
            issue.Comments.Add(comment);

            await _context.SaveChangesAsync();

            return Created($"/projects/{projectId}/issues/{issue.Id}/comments/{comment.Id}", new {
                Id = comment.Id
            });
        }
    }
}