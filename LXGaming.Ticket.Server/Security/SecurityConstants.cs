namespace LXGaming.Ticket.Server.Security {

    public static class SecurityConstants {

        public static class Claims {

            public const string Scope = "scope";
        }

        public static class Scopes {

            // Issue
            public const string IssueRead = "issue.read";
            public const string IssueWrite = "issue.write";

            // Issue Comment
            public const string IssueCommentRead = "issue.comment.read";
            public const string IssueCommentWrite = "issue.comment.write";

            // Project
            public const string ProjectRead = "project.read";

            // User
            public const string UserRead = "user.read";
            public const string UserWrite = "user.write";
        }
    }
}