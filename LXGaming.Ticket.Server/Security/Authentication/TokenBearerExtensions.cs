using Microsoft.AspNetCore.Authentication;

namespace LXGaming.Ticket.Server.Security.Authentication {

    public static class TokenBearerExtensions {

        public static AuthenticationBuilder AddTokenBearer(this AuthenticationBuilder builder) {
            return builder.AddScheme<TokenBearerOptions, TokenBearerHandler>(TokenBearerDefaults.AuthenticationScheme, options => { });
        }
    }
}