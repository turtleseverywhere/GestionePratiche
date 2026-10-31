using Microsoft.IdentityModel.Tokens;
using System.Net;

namespace sistema_gestione_pratiche.Common
{
    public static class ConstantValues
    {
        public static class Auth
        {            
            public const string AuthSettings = "AuthSettings";
            public const string DomainName = "DomainName";
            public const string DomainHost = "DomainHost";
            public const string DomainPort = "DomainPort";
            public const string ConnectionTimeout = "ConnectionTimeout";
            public const string UseSSL = "UseSSL";
            public const string CheckOnAD = "CheckOnAD";
            public const string SecurityStampClaimName = "AspNet.Identity.SecurityStamp";
            public const string JWT = "JWT";
            public const string JWTIssuer = "Issuer";
            public const string JWTAudience = "Audience";
            public const string JWTEncryptionKey = "EncryptionKey";
            public const string JWTSigningKey = "SigningKey";
            public const string JWTExpiry = "Expiry";
            public const string JWTRefreshTokenExpiry = "RefreshTokenExpiry";
            public const string JWTScheme = "JWTScheme";
            public const string LDAP = "LDAP";
            public static TokenValidationParameters TokenValidationParameters { get; set; } = new TokenValidationParameters();
            public static class Claims
            {
                public static class Types
                {
                    public const string CANDOANYTHING = "CANDOANYTHING";
                    public const string CanRegisterToSignalR = "CanRegisterToSignalR";
                    public const string CanSendSignalRMessageToBroadcast = "CanSendSignalRMessageToBroadcast";
                    public const string CanSendSignalRMessageToClient = "CanSendSignalRMessageToClient";
                    public const string CanSendSignalRMessageToConnection = "CanSendSignalRMessageToConnection";
                    public const string CanSendSignalRMessageToUser = "CanSendSignalRMessageToUser";
                    public const string CanUseNotifications = "CanUseNotifications";
                }
                public static class Values
                {
                    public const string True = "True";
                    public const string False = "False";
                }
            }

        }

        public static class ServicesHttpResponses
        {
            public static readonly (object Result, HttpStatusCode StatusCode) BadRequest = ("BadRequest", HttpStatusCode.BadRequest);
            public static readonly (object Result, HttpStatusCode StatusCode) InternalServerError = ("InternalServerError", HttpStatusCode.InternalServerError);
            public static readonly (object Result, HttpStatusCode StatusCode) ItemDoesntExist = ("ItemDoesntExist", HttpStatusCode.BadRequest);
            public static readonly (object Result, HttpStatusCode StatusCode) LoggedIn = ("LoggedIn", HttpStatusCode.OK);
            public static readonly (object Result, HttpStatusCode StatusCode) LoginFailed = ("LoginFailed", HttpStatusCode.BadRequest);
            public static readonly (object Result, HttpStatusCode StatusCode) OK = ("OK", HttpStatusCode.OK);
            public static readonly (object Result, HttpStatusCode StatusCode) UserDoesntExist = ("UserDoesntExist", HttpStatusCode.BadRequest);
        }
    }
}
