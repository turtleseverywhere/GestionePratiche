using sistema_gestione_pratiche.Common;
using sistema_gestione_pratiche.Models;
using sistema_gestione_pratiche.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace sistema_gestione_pratiche.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SignalRController : ControllerBase
    {
        private readonly IConfiguration configuration;
        protected readonly ILogger<LogController> logger;
        private readonly SignalRService signalRService;

        public SignalRController(IConfiguration configuration, ILogger<LogController> logger, SignalRService signalRService)
        {
            this.configuration = configuration;
            this.logger = logger;
            this.signalRService = signalRService;
        }

        [HttpPost("SendMessageToBroadcast")]
        [Authorize(Policy = ConstantValues.Auth.Claims.Types.CanSendSignalRMessageToBroadcast)]
        public async Task<object> SendMessageToBroadcast([FromBody] SignalRMessageModel model)
        {
            try
            {
                if (model.Message != null)
                    await this.signalRService.SendMessageToBroadcast(model.Message);
                return 200;
            }
            catch (Exception ex)
            {
                this.logger.Log(LogLevel.Error, ex, string.Empty);
                Response.StatusCode = (int)ConstantValues.ServicesHttpResponses.InternalServerError.StatusCode;
                return ConstantValues.ServicesHttpResponses.InternalServerError.Result;
            }
        }

        [HttpPost("SendMessageToConnection")]
        [Authorize(Policy = ConstantValues.Auth.Claims.Types.CanSendSignalRMessageToConnection)]
        public async Task<object> SendMessageToConnection([FromBody] SignalRMessageModel model)
        {
            try
            {
                if (model.ConnectionId != null && model.Message != null)
                    await this.signalRService.SendMessageToConnection(model.ConnectionId, model.Message);
                return 200;
            }
            catch (Exception ex)
            {
                this.logger.Log(LogLevel.Error, ex, string.Empty);
                Response.StatusCode = (int)ConstantValues.ServicesHttpResponses.InternalServerError.StatusCode;
                return ConstantValues.ServicesHttpResponses.InternalServerError.Result;
            }
        }

        [HttpPost("SendMessageToClient")]
        [Authorize(Policy = ConstantValues.Auth.Claims.Types.CanSendSignalRMessageToClient)]
        public async Task<object> SendMessageToClient([FromBody] SignalRMessageModel model)
        {
            try
            {
                if (model.ClientId != null && model.Message != null)
                    await this.signalRService.SendMessageToClient(model.ClientId, model.Message);
                return 200;
            }
            catch (Exception ex)
            {
                this.logger.Log(LogLevel.Error, ex, string.Empty);
                Response.StatusCode = (int)ConstantValues.ServicesHttpResponses.InternalServerError.StatusCode;
                return ConstantValues.ServicesHttpResponses.InternalServerError.Result;
            }
        }

        [HttpPost("SendMessageToUser")]
        [Authorize(Policy = ConstantValues.Auth.Claims.Types.CanSendSignalRMessageToUser)]
        public async Task<object> SendMessageToUser([FromBody] SignalRMessageModel model)
        {
            try
            {
                if (model.UserId != null && model.Message != null)
                    await this.signalRService.SendMessageToUser(model.UserId, model.Message);
                return 200;
            }
            catch (Exception ex)
            {
                this.logger.Log(LogLevel.Error, ex, string.Empty);
                Response.StatusCode = (int)ConstantValues.ServicesHttpResponses.InternalServerError.StatusCode;
                return ConstantValues.ServicesHttpResponses.InternalServerError.Result;
            }
        }
    }
}