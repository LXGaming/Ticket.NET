using System.ComponentModel.DataAnnotations;

namespace LXGaming.Ticket.Server.Models {

    public class Identifier {

        [Key, MaxLength(255)]
        public string Id { get; init; }

        [Required, MaxLength(255)]
        public string Name { get; init; }
    }
}