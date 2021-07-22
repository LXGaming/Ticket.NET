using System;
using System.ComponentModel.DataAnnotations;

namespace LXGaming.Ticket.Server.Models {

    public class IssueComment {

        [Key]
        public ulong Id { get; init; }

        [Required]
        public ulong IssueId { get; init; }

        [Required]
        public ulong UserId { get; init; }

        [Required]
        public string Body { get; init; }

        [Required]
        public DateTime CreatedAt { get; init; }

        [Required]
        public DateTime UpdatedAt { get; set; }

        public virtual Issue Issue { get; init; }

        public virtual User User { get; init; }
    }
}