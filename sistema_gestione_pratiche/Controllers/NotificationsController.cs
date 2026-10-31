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
    [Authorize(Policy = ConstantValues.Auth.Claims.Types.CanUseNotifications)]
    public class NotificationsController : ControllerBase
    {
        protected readonly ILogger<AuthController> logger;
        private NotificationsService notificationsService;
        public NotificationsController(ILogger<AuthController> logger, NotificationsService notificationsService)
        {
            this.logger = logger;
            this.notificationsService = notificationsService;
        }
        [HttpGet("GetNotifications")]
        public async Task<object> GetNotifications()
        {
            try
            {
                var result = await this.notificationsService.GetNotifications(User);
                Response.StatusCode = (int)result.StatusCode;
                return new
                {
                    notifications = result.Result
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
        [HttpPost("SetUnreadNotificationsAsRead")]
        public async Task<object> SetUnreadNotificationsAsRead()
        {
            try
            {
                var result = await this.notificationsService.SetUnreadNotificationsAsRead(User);
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
        [HttpPost("DeleteNotification")]
        public async Task<object> DeleteNotification(NotificationModel notification)
        {
            try
            {
                var result = await this.notificationsService.DeleteNotification(notification.Id);
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