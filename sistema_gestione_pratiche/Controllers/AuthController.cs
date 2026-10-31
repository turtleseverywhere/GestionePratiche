using sistema_gestione_pratiche.Common;
using sistema_gestione_pratiche.Models;
using sistema_gestione_pratiche.Models.AuthModels;
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
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration configuration;
        protected readonly ILogger<AuthController> logger;
        public AuthController(IConfiguration configuration, ILogger<AuthController> logger)
        {
            this.configuration = configuration;
            this.logger = logger;
        }
        [HttpPost("Login")]
        public async Task<object> Login([FromBody] LoginModel model,
            [FromServices] AuthService authService)
        {
            try
            {
                var result = await authService.Login(model.Username, model.Password);
                Response.StatusCode = (int)result.StatusCodeResult.StatusCode;
                var toSender = new
                {
                    Token = result.LoginResult?.Token,
                    RefreshToken = result.LoginResult?.RefreshToken,
                    Claims = result.LoginResult?.Claims
                };
                return toSender;
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
        [Authorize]
        [HttpPost("RefreshTokens")]
        public async Task<object> RefreshTokens([FromBody] LoginModel model,
            [FromServices] AuthService authService)
        {
            try
            {
                var result = await authService.RefreshJWTTokens(model.RefreshToken);
                Response.StatusCode = (int)result.StatusCodeResult.StatusCode;
                var toSender = new
                {
                    Token = result.LoginResult.Token,
                    RefreshToken = result.LoginResult.RefreshToken,
                    Claims = result.LoginResult.Claims
                };
                return toSender;
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
        [HttpPost("Logout")]
        public async Task<object> Logout([FromServices] AuthService authService)
        {
            try
            {
                var result = await authService.Logout(User);
                Response.StatusCode = (int)result.StatusCode;
                return result;
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
        [HttpPost("GetUsers")]
        [Authorize(Policy = ConstantValues.Auth.Claims.Types.CANDOANYTHING)]
        public async Task<object> GetUsers([FromServices] AuthService authService,
            [FromBody] UsersAndRolesModel model)
        {
            try
            {
                var result = authService.GetUsersWithFilter(model.UserName, model.Email);
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

        [HttpPost("InsertUser")]
        [Authorize(Policy = ConstantValues.Auth.Claims.Types.CANDOANYTHING)]
        public async Task<object> InsertUser([FromServices] AuthService authService,
            [FromBody] UsersAndRolesModel model)
        {
            try
            {
                var result = await authService.InsertUser(model.UserName, model.Email, model.Password);
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

        [HttpPost("UpdateUser")]
        [Authorize(Policy = ConstantValues.Auth.Claims.Types.CANDOANYTHING)]
        public async Task<object> UpdateUser([FromServices] AuthService authService,
            [FromBody] UsersAndRolesModel model)
        {
            try
            {
                var result = await authService.UpdateUser(model.UserID, model.UserName, model.Email, model.Password);
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

        [HttpPost("DeleteUser")]
        [Authorize(Policy = ConstantValues.Auth.Claims.Types.CANDOANYTHING)]
        public async Task<object> DeleteUser([FromServices] AuthService authService,
            [FromBody] UsersAndRolesModel model)
        {
            try
            {
                var result = await authService.DeleteUser(model.UserID);
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

        [HttpPost("AddRoleToUser")]
        [Authorize(Policy = ConstantValues.Auth.Claims.Types.CANDOANYTHING)]
        public async Task<object> AddRoleToUser([FromServices] AuthService authService,
           [FromBody] UsersAndRolesModel model)
        {
            try
            {
                var result = await authService.AddRoleToUser(model.UserID, model.RoleName);
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

        [HttpPost("RemoveRoleFromUser")]
        [Authorize(Policy = ConstantValues.Auth.Claims.Types.CANDOANYTHING)]
        public async Task<object> RemoveRoleFromUser([FromServices] AuthService authService,
          [FromBody] UsersAndRolesModel model)
        {
            try
            {
                var result = await authService.RemoveRoleFromUser(model.UserID, model.RoleName);
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

        [HttpPost("AddClaimToUser")]
        [Authorize(Policy = ConstantValues.Auth.Claims.Types.CANDOANYTHING)]
        public async Task<object> AddClaimToUser([FromServices] AuthService authService,
          [FromBody] UsersAndRolesModel model)
        {
            try
            {
                var result = await authService.AddClaimToUser(model.UserID, model.ClaimType, model.ClaimValue);
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

        [HttpPost("RemoveClaimFromUser")]
        [Authorize(Policy = ConstantValues.Auth.Claims.Types.CANDOANYTHING)]
        public async Task<object> RemoveClaimFromUser([FromServices] AuthService authService,
         [FromBody] UsersAndRolesModel model)
        {
            try
            {
                var result = await authService.RemoveClaimFromUser(model.UserID, model.ClaimType, model.ClaimValue);
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

        [HttpPost("GetAvailableClaimsForUser")]
        [Authorize(Policy = ConstantValues.Auth.Claims.Types.CANDOANYTHING)]
        public async Task<object> GetAvailableClaimsForUser([FromServices] AuthService authService,
        [FromBody] UsersAndRolesModel model)
        {
            try
            {
                var result = await authService.GetAvailableClaimsForUser(model.UserID);
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
        [HttpPost("GetAvailableRolesForUser")]
        [Authorize(Policy = ConstantValues.Auth.Claims.Types.CANDOANYTHING)]
        public async Task<object> GetAvailableRolesForUser([FromServices] AuthService authService,
            [FromBody] UsersAndRolesModel model)
        {
            try
            {
                var result = await authService.GetAvailableRolesForUser(model.UserID);
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

        [HttpPost("GetRoles")]
        [Authorize(Policy = ConstantValues.Auth.Claims.Types.CANDOANYTHING)]
        public async Task<object> GetRoles([FromServices] AuthService authService,
           [FromBody] UsersAndRolesModel model)
        {
            try
            {
                var result = authService.GetRolesWithFilter(model.RoleName);
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

        [HttpPost("InsertRole")]
        [Authorize(Policy = ConstantValues.Auth.Claims.Types.CANDOANYTHING)]
        public async Task<object> InsertRole([FromServices] AuthService authService,
           [FromBody] UsersAndRolesModel model)
        {
            try
            {
                var result = await authService.InsertRole(model.RoleName);
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

        [HttpPost("UpdateRole")]
        [Authorize(Policy = ConstantValues.Auth.Claims.Types.CANDOANYTHING)]
        public async Task<object> UpdateRole([FromServices] AuthService authService,
           [FromBody] UsersAndRolesModel model)
        {
            try
            {
                var result = await authService.UpdateRole(model.RoleID, model.RoleName);
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

        [HttpPost("DeleteRole")]
        [Authorize(Policy = ConstantValues.Auth.Claims.Types.CANDOANYTHING)]
        public async Task<object> DeleteRole([FromServices] AuthService authService,
            [FromBody] UsersAndRolesModel model)
        {
            try
            {
                var result = await authService.DeleteRole(model.RoleID);
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

        [HttpPost("AddClaimToRole")]
        [Authorize(Policy = ConstantValues.Auth.Claims.Types.CANDOANYTHING)]
        public async Task<object> AddClaimToRole([FromServices] AuthService authService,
         [FromBody] UsersAndRolesModel model)
        {
            try
            {
                var result = await authService.AddClaimToRole(model.RoleID, model.ClaimType, model.ClaimValue);
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

        [HttpPost("RemoveClaimFromRole")]
        [Authorize(Policy = ConstantValues.Auth.Claims.Types.CANDOANYTHING)]
        public async Task<object> RemoveClaimFromRole([FromServices] AuthService authService,
         [FromBody] UsersAndRolesModel model)
        {
            try
            {
                var result = await authService.RemoveClaimFromRole(model.RoleID, model.ClaimType, model.ClaimValue);
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

        [HttpPost("GetAvailableClaimsForRole")]
        [Authorize(Policy = ConstantValues.Auth.Claims.Types.CANDOANYTHING)]
        public async Task<object> GetAvailableClaimsForRole([FromServices] AuthService authService,
         [FromBody] UsersAndRolesModel model)
        {
            try
            {
                var result = await authService.GetAvailableClaimsForRole(model.RoleID);
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
