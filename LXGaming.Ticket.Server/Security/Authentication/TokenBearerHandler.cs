using System;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using LXGaming.Ticket.Server.Storage;
using LXGaming.Ticket.Server.Util;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LXGaming.Ticket.Server.Security.Authentication {

    public class TokenBearerHandler : AuthenticationHandler<TokenBearerOptions> {

        private readonly StorageContext _context;

        public TokenBearerHandler(IOptionsMonitor<TokenBearerOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock, StorageContext context)
            : base(options, logger, encoder, clock) {
            _context = context;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync() {
            string authorization = Request.Headers["Authorization"];
            if (string.IsNullOrEmpty(authorization)) {
                return AuthenticateResult.NoResult();
            }

            if (!authorization.StartsWith(Scheme.Name, StringComparison.OrdinalIgnoreCase)) {
                return AuthenticateResult.NoResult();
            }

            var value = authorization[Scheme.Name.Length..].Trim();
            if (string.IsNullOrEmpty(value)) {
                return AuthenticateResult.NoResult();
            }

            var hashedValue = HashUtils.CreateSha256(value);
            var token = await _context.Tokens.SingleOrDefaultAsync(model => string.Equals(model.Value, hashedValue));
            if (token == null) {
                return AuthenticateResult.NoResult();
            }

            var claims = new[] {
                new Claim(SecurityConstants.Claims.Scope, token.Scopes)
            };

            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);
            return AuthenticateResult.Success(ticket);
        }
    }
}