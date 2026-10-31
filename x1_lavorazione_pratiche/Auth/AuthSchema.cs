using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace x1_lavorazione_pratiche.Auth
{
    public class AuthSchema : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public AuthSchema(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock) : base(options, logger, encoder, clock)
        {
        }
        /*nel caso servisse sovrascrivere la funzione di autenticazione..*/
        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            //per il success.. ovviamente da prendere i claim dal db..
            //var claims = new[] { new Claim("name", "asd") };
            //var identity = new ClaimsIdentity(claims, "JWT");
            //var claimsPrincipal = new ClaimsPrincipal(identity);
            //return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(claimsPrincipal, Scheme.Name)));

            //per il fail..
            //questo comunque lo rimette a 401..
            //Response.StatusCode = 504;
            //return Task.FromResult(AuthenticateResult.Fail("LOLNO"));
            throw new NotImplementedException();
        }
    }
}
