using System;
using System.ComponentModel.DataAnnotations;

namespace LXGaming.Ticket.Server.Models {

    public class ApiToken {

        [Key]
        public ulong Id { get; init; }

        [Required, MaxLength(255)]
        public string Name { get; set; }

        [Required, MaxLength(64)]
        public string Value { get; set; }

        [Required]
        public string Scopes { get; set; }

        [Required]
        public DateTime CreatedAt { get; init; }

        [Required]
        public DateTime UpdatedAt { get; set; }
    }
}