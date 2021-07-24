using System.Threading.Tasks;
using AsyncEvent;
using LXGaming.Ticket.Server.Models;
using LXGaming.Ticket.Server.Services.Event.Models;
using Serilog;

namespace LXGaming.Ticket.Server.Services.Event {

    public class EventService {

        public event AsyncEventHandler<Issue> IssueCreated;
        public event AsyncEventHandler<IssueUpdatedEventArgs> IssueUpdated;
        public event AsyncEventHandler<IssueComment> IssueCommentCreated;
        public event AsyncEventHandler<UserUpdatedEventArgs> UserUpdated;

        public Task OnIssueCreatedAsync(Issue issue) {
            return InvokeAsync(IssueCreated, issue);
        }

        public Task OnIssueUpdatedAsync(IssueUpdatedEventArgs eventArgs) {
            return InvokeAsync(IssueUpdated, eventArgs);
        }

        public Task OnIssueCommentCreatedAsync(IssueComment comment) {
            return InvokeAsync(IssueCommentCreated, comment);
        }

        public Task OnUserUpdatedAsync(UserUpdatedEventArgs eventArgs) {
            return InvokeAsync(UserUpdated, eventArgs);
        }

        private Task InvokeAsync<T>(AsyncEventHandler<T> eventHandler, T eventArgs) {
            eventHandler.InvokeAsync(this, eventArgs).ContinueWith(task => {
                Log.Error(task.Exception, "Encountered an error while invoking event");
            }, TaskContinuationOptions.OnlyOnFaulted);
            return Task.CompletedTask;
        }
    }
}