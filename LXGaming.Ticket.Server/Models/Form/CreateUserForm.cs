using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LXGaming.Ticket.Server.Models.Form {

    public class CreateUserForm {

        [Required, MinLength(1)]
        public Dictionary<string, string> Identifiers { get; init; }

        [Required, MaxLength(1), MinLength(1)]
        public Dictionary<string, string> Names { get; init; }
    }
}