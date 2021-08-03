using System;
using System.Collections.Generic;
using System.Linq;
using LXGaming.Ticket.Server.Models;

namespace LXGaming.Ticket.Server.Util {

    public static class Extensions {

        #region Comment
        public static DateTime GetCreatedAtUtc(this IssueComment comment) {
            return comment.CreatedAt.ToUniversalTime();
        }

        public static DateTime GetUpdatedAtUtc(this IssueComment comment) {
            return comment.UpdatedAt.ToUniversalTime();
        }

        public static object GetUser(this IssueComment comment) {
            return GetUser(comment.User, comment.Issue.ProjectId);
        }
        #endregion

        #region Issue
        public static DateTime GetCreatedAtUtc(this Issue issue) {
            return issue.CreatedAt.ToUniversalTime();
        }

        public static DateTime GetUpdatedAtUtc(this Issue issue) {
            return issue.UpdatedAt.ToUniversalTime();
        }

        public static object GetUser(this Issue issue) {
            return GetUser(issue.User, issue.ProjectId);
        }

        public static string GetUserNameValue(this Issue issue, string defaultValue = null) {
            return GetUserNameValue(issue.User, issue.ProjectId);
        }

        public static UserName GetUserName(this Issue issue) {
            return GetUserName(issue.User, issue.ProjectId);
        }
        #endregion

        #region User
        public static DateTime GetCreatedAtUtc(this User user) {
            return user.CreatedAt.ToUniversalTime();
        }

        public static DateTime GetUpdatedAtUtc(this User user) {
            return user.UpdatedAt.ToUniversalTime();
        }

        public static Dictionary<string, string> GetIdentifiers(this User user) {
            return user.Identifiers.ToDictionary(model => model.IdentifierId, model => model.Value);
        }

        public static Dictionary<string, string> GetNames(this User user, string projectId = null) {
            return user.Names
                .Where(model => projectId == null || string.Equals(model.ProjectId, projectId))
                .ToDictionary(model => model.ProjectId, model => model.Value);
        }

        private static object GetUser(this User user, string projectId) {
            return new {
                Id = user.Id,
                Names = user.GetNames(projectId)
            };
        }

        public static string GetUserNameValue(this User user, string projectId, string defaultValue = null) {
            return GetUserName(user, projectId)?.Value ?? defaultValue;
        }

        public static UserName GetUserName(this User user, string projectId) {
            return user.Names?.SingleOrDefault(model => string.Equals(model.ProjectId, projectId));
        }
        #endregion
    }
}