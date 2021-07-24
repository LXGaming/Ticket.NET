using LXGaming.Ticket.Server.Models;

namespace LXGaming.Ticket.Server.Services.Event.Models {

    public class IssueUpdatedEventArgs {

        public readonly Issue Issue;
        public readonly IssueStatus PreviousState;

        public IssueUpdatedEventArgs(Issue issue) {
            Issue = issue;
            PreviousState = issue.Status;
        }
    }
}