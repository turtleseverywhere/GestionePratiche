using sistema_gestione_pratiche.DB.Entities.AuthEntities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace sistema_gestione_pratiche.Auth
{
    public class AuthMiddleware
    {
        private readonly RequestDelegate next;
        private readonly IConfiguration configuration;

        public AuthMiddleware(RequestDelegate next, IConfiguration configuration)
        {
            this.next = next;
            this.configuration = configuration;
        }

        public async Task Invoke(HttpContext httpContext, SignInManager<User> signInManager)
        {
            if (httpContext.User.Identity.IsAuthenticated)
            {
                /*questo verifica ad ogni chiamata che il token sia sempre all'ultima versione, di modo che se nel frattempo
                i permessi dell'utente sono cambiati l'utente viene sloggato*/
                var res = await signInManager.ValidateSecurityStampAsync(httpContext.User);
                if (res == null)
                {

                    //await signInManager.SignOutAsync();
                    httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return;

                }
                await next(httpContext);
                return;
            }
            else
            {
                await next(httpContext);
                return;
            }
        }
    }
}