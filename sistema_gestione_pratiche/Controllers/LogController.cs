using sistema_gestione_pratiche.Common;
using sistema_gestione_pratiche.Models;
using sistema_gestione_pratiche.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace sistema_gestione_pratiche.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LogController : ControllerBase
    {
        private readonly IConfiguration configuration;
        protected readonly ILogger<LogController> logger;

        public LogController(IConfiguration configuration, ILogger<LogController> logger)
        {
            this.configuration = configuration;
            this.logger = logger;
        }
        [HttpPost("Log")]
        public async Task<object> Log([FromBody] LogModel model,
            [FromServices] AuthService authService)
        {
            try
            {
                this.logger.Log(LogLevel.Error,
                    "Timestamp: " + model.TimeStamp
                    + " FileName: " + model.FileName
                    + " LineNumber: " + model.LineNumber
                    + " ColumnNumber: " + model.ColumnNumber
                    + " Message: " + model.Message
                    + " Level: " + model.Level
                    + " Additional: " + model.Additional);
                System.Diagnostics.Debug.WriteLine(model.Message);
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