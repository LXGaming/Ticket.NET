using System.ComponentModel.DataAnnotations;

namespace LXGaming.Ticket.Server.Models {

    public class UserIdentifier {

        [Key]
        public ulong Id { get; init; }

        [Required]
        public string IdentifierId { get; init; }

        [Required]
        public ulong UserId { get; init; }

        [Required, MaxLength(255)]
        public string Value { get; init; }

        public virtual Identifier Identifier { get; init; }

        public virtual User User { get; init; }
    }
}