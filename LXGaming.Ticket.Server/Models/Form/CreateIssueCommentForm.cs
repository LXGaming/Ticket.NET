using System.ComponentModel.DataAnnotations;

namespace LXGaming.Ticket.Server.Models.Form {

    public class CreateIssueCommentForm {

        [Required]
        public ulong UserId { get; init; }

        [Required]
        public string Body { get; init; }
    }
}