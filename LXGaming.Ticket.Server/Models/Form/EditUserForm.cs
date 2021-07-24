using System.Collections.Generic;

namespace LXGaming.Ticket.Server.Models.Form {

    public class EditUserForm {

        public bool? Banned { get; init; }

        public Dictionary<string, string> Identifiers { get; init; }

        public Dictionary<string, string> Projects { get; init; }
    }
}