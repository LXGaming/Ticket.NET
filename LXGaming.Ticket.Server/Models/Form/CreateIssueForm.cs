using System.ComponentModel.DataAnnotations;

namespace LXGaming.Ticket.Server.Models.Form {

    public class CreateIssueForm {

        [Required]
        public ulong UserId { get; init; }

        public string Data { get; init; }

        [Required]
        public string Body { get; init; }
    }
}