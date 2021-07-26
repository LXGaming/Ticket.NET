using System.Linq;
using LXGaming.Ticket.Server.Models;

namespace LXGaming.Ticket.Server.Util {

    public static class Extensions {

        public static string GetUserNameValue(this Issue issue, string defaultValue = null) {
            return GetUserNameValue(issue.User, issue.ProjectId);
        }

        public static UserName GetUserName(this Issue issue) {
            return GetUserName(issue.User, issue.ProjectId);
        }

        public static string GetUserNameValue(this User user, string projectId, string defaultValue = null) {
            return GetUserName(user, projectId)?.Value ?? defaultValue;
        }

        public static UserName GetUserName(this User user, string projectId) {
            return user.Names?.SingleOrDefault(model => string.Equals(model.ProjectId, projectId));
        }
    }
}