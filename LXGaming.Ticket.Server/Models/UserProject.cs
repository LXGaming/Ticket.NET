using System.ComponentModel.DataAnnotations;

namespace LXGaming.Ticket.Server.Models {

    public class UserProject {

        [Key]
        public ulong Id { get; init; }

        [Required]
        public string ProjectId { get; init; }

        [Required]
        public ulong UserId { get; init; }

        [Required, MaxLength(255)]
        public string Value { get; set; }

        public virtual Project Project { get; init; }

        public virtual User User { get; init; }
    }
}