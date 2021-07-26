using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LXGaming.Ticket.Server.Models {

    public class User {

        [Key]
        public ulong Id { get; init; }

        [Required]
        public bool Banned { get; set; }

        [Required]
        public DateTime CreatedAt { get; init; }

        [Required]
        public DateTime UpdatedAt { get; set; }

        public virtual List<UserIdentifier> Identifiers { get; init; }

        public virtual List<UserName> Names { get; init; }
    }
}