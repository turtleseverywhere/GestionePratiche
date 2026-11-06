using x1_lavorazione_pratiche.Common;
using x1_lavorazione_pratiche.Models;
using x1_lavorazione_pratiche.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace x1_lavorazione_pratiche.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = ConstantValues.Auth.Claims.Types.CanManagePratiche)]
    public class PraticheController : ControllerBase
    {
        protected readonly ILogger<AuthController> logger;
        private PraticheService praticheService;
        public PraticheController(ILogger<AuthController> logger, PraticheService praticheService)
        {
            this.logger = logger;
            this.praticheService = praticheService;
        }

        [HttpPut("ChangePraticaStatus")]
        public async Task<object> ChangePraticaStatus(PraticaModel pratica)
        {
            try
            {
                var result = await this.praticheService.ChangePraticaStatus(pratica);
                Response.StatusCode = (int)result.StatusCode;
                return new
                {
                    result = result.Result
                };
            }
            catch (Exception ex)
            {
                this.logger.Log(LogLevel.Error, ex, string.Empty);
                Response.StatusCode = (int)ConstantValues.ServicesHttpResponses.InternalServerError.StatusCode;
                return new
                {
                    result = ConstantValues.ServicesHttpResponses.InternalServerError.Result
                };
            }
        }
    }
}