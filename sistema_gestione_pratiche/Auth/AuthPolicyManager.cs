using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using sistema_gestione_pratiche.Helpers;
using sistema_gestione_pratiche.Common;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Principal;

namespace sistema_gestione_pratiche.Auth
{
    public class AuthPolicyManager
    {
        public class AuthPolicyRequirement : IAuthorizationRequirement
        {
            public string Claim { get; set; }
            public AuthPolicyRequirement(string claim)
            {
                this.Claim = claim;
            }
        }
        public class AuthPolicyHandler : AuthorizationHandler<AuthPolicyRequirement>
        {
            private IHttpContextAccessor httpContextAccessor;
            private readonly IConfiguration configuration;
            public AuthPolicyHandler(IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
            {
                this.httpContextAccessor = httpContextAccessor;
                this.configuration = configuration;
            }
            protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AuthPolicyRequirement requirement)
            {
                HttpContext httpContext = httpContextAccessor.HttpContext;

                if (httpContext.User.CheckPolicy(requirement.Claim))
                    context.Succeed(requirement);
                else
                {
                    //questo token lo manda signalr..
                    if (httpContext.Request.Query.ContainsKey("access_token"))
                    {
                        var jwtToken = httpContext.Request.Query["access_token"];
                        SecurityToken? validatedToken = null;
                        var tokenHandler = new JwtSecurityTokenHandler();

                        IPrincipal principal = tokenHandler.ValidateToken(jwtToken, ConstantValues.Auth.TokenValidationParameters, out validatedToken);
                        if (((ClaimsPrincipal)principal).CheckPolicy(requirement.Claim))
                            context.Succeed(requirement);
                    }
                    else
                    {
                        httpContext.Response.StatusCode = 401;
                        //httpContext.Response.WriteAsync(string.Empty);
                        /*questo completeAsync funziona in debug ma non in release :) GG microsoft*/
                        //httpContext.Response.CompleteAsync();
                        context.Fail();
                    }
                }
                return Task.FromResult(0);
            }
        }
    }
}