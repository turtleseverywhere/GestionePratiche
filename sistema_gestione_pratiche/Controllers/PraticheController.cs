using sistema_gestione_pratiche.Common;
using sistema_gestione_pratiche.Models;
using sistema_gestione_pratiche.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace sistema_gestione_pratiche.Controllers
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
        [HttpGet("GetPratiche")]
        public async Task<object> GetPratiche()
        {
            try
            {
                var result = await this.praticheService.GetPratiche(User);
                Response.StatusCode = (int)result.StatusCode;
                return result.Result;
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
        [HttpPost("InsertPratica")]
        public async Task<object> InsertPratica([FromBody] PraticaModel pratica)
        {
            try
            {
                var result = await this.praticheService.InsertPratica(User, pratica);
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
        [HttpPost("UpdatePratica")]
        public async Task<object> UpdatePratica(PraticaModel pratica)
        {
            try
            {
                var result = await this.praticheService.UpdatePratica(pratica);
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

        [HttpPost("ChangePraticaStatus")]
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

        [HttpPost("DeletePratica")]
        public async Task<object> DeletePratica(PraticaModel pratica)
        {
            try
            {
                var result = await this.praticheService.DeletePratica(pratica);
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

        [HttpPost("DownloadPratica")]
        public async Task<object> DownloadPratica(PraticaModel pratica)
        {
            try
            {
                var result = await this.praticheService.DownloadPratica(pratica);
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

        [HttpPost("GetPraticaHistory")]
        public async Task<object> GetPraticaHistory(PraticaModel pratica)
        {
            try
            {
                var result = await this.praticheService.GetPraticaHistory(pratica);
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