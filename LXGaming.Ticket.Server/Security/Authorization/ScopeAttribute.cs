using System;
using System.Linq;
using System.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace LXGaming.Ticket.Server.Security.Authorization {

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class ScopeAttribute : AuthorizeAttribute, IAuthorizationFilter {

        private readonly string[] _scopes;

        public ScopeAttribute(params string[] scopes) : base(SecurityConstants.Policies.Scope) {
            _scopes = scopes;
        }

        public void OnAuthorization(AuthorizationFilterContext context) {
            var user = context.HttpContext.User;
            if (user.Identity == null || !user.Identity.IsAuthenticated) {
                return;
            }

            var scope = user.FindFirstValue(SecurityConstants.Claims.Scope);
            if (string.IsNullOrEmpty(scope)) {
                context.Result = new StatusCodeResult((int) HttpStatusCode.Forbidden);
                return;
            }

            var scopes = scope.Split(",");
            if (!scopes.Any(s => _scopes.Contains(s.Trim()))) {
                context.Result = new StatusCodeResult((int) HttpStatusCode.Forbidden);
            }
        }
    }
}