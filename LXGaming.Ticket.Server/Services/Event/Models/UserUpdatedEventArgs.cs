using LXGaming.Ticket.Server.Models;

namespace LXGaming.Ticket.Server.Services.Event.Models {

    public class UserUpdatedEventArgs {

        public readonly User User;
        public readonly bool PreviousBanned;

        public UserUpdatedEventArgs(User user) {
            User = user;
            PreviousBanned = user.Banned;
        }
    }
}