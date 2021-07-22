using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LXGaming.Ticket.Server.Models {

    public class Issue {

        [Key]
        public ulong Id { get; init; }

        [Required]
        public string ProjectId { get; init; }

        [Required]
        public ulong UserId { get; init; }

        public string Data { get; init; }

        [Required]
        public string Body { get; init; }

        [Required]
        public IssueStatus Status { get; set; }

        [Required]
        public bool Read { get; set; }

        [Required]
        public DateTime CreatedAt { get; init; }

        [Required]
        public DateTime UpdatedAt { get; set; }

        public virtual List<IssueComment> Comments { get; init; }

        public virtual Project Project { get; init; }

        public virtual User User { get; init; }
    }
}