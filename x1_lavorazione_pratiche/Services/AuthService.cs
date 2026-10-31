using Microsoft.AspNetCore.Identity;
using System.Data;
using x1_lavorazione_pratiche.DB.Entities.AuthEntities;
using x1_lavorazione_pratiche.Common;
using System.Net;
using x1_lavorazione_pratiche.DB.Contexts;
using Microsoft.EntityFrameworkCore;
using x1_lavorazione_pratiche.Helpers;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using System;
using System.Linq;
using System.Collections.Generic;
using x1_lavorazione_pratiche.Models.AuthModels;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Principal;
using Novell.Directory.Ldap;

namespace x1_lavorazione_pratiche.Services
{
    public class AuthService
    {
        IConfiguration configuration;
        UserManager<User> userManager;
        RoleManager<Role> roleManager;
        AuthDBContext authDBContext;
        public AuthService(IConfiguration configuration,
            UserManager<User> userManager,
            RoleManager<Role> roleManager,
            AuthDBContext authDBContext)
        {
            this.configuration = configuration;
            this.userManager = userManager;
            this.roleManager = roleManager;
            this.authDBContext = authDBContext;
        }

        public async Task<((List<ClaimModel> Claims, string Token, string RefreshToken)? LoginResult,
            (object StatusCodeMessage, HttpStatusCode StatusCode) StatusCodeResult)>
            Login(string userName, string password)
        {
            try
            {
                string? checkOnAD = configuration.GetSection(ConstantValues.Auth.AuthSettings)
                    .GetSection(ConstantValues.Auth.LDAP)
                    .GetSection(ConstantValues.Auth.CheckOnAD).Value;

                bool userExistsOnActiveDirectory = false;
                if (bool.Parse(checkOnAD))
                {
                    /*
                    try
                    {
                        //questo è disponibile solo su windows, non su altri sistemi operativi
                        using (PrincipalContext pc = new PrincipalContext(ContextType.Domain, domain))
                        {
                            userExistsOnActiveDirectory = pc.ValidateCredentials(userName, password);
                        }
                    }
                    catch (Exception ex)
                    {
                    }
                    */
                    //questo è supportato su windows e linux (ma non l'ho provato su linux), ma non android e ios
                    /*
                    LdapConnection ldapConnection = new LdapConnection(domain);
                    try
                    {
                        ldapConnection.Bind(new NetworkCredential(userName, password));
                        userExistsOnActiveDirectory = true;
                    }
                    catch (Exception ex)
                    {
                    }
                    finally
                    {
                        ldapConnection.Dispose();
                    }
                    */
                    var LDAPConfigSection = configuration.GetSection(ConstantValues.Auth.AuthSettings)
                        .GetSection(ConstantValues.Auth.LDAP);

                    string? domainName = LDAPConfigSection.GetSection(ConstantValues.Auth.DomainName).Value;
                    string? domainHost = LDAPConfigSection.GetSection(ConstantValues.Auth.DomainHost).Value;
                    int domainPort = int.Parse(LDAPConfigSection.GetSection(ConstantValues.Auth.DomainPort).Value);
                    bool useSSL = bool.Parse(LDAPConfigSection.GetSection(ConstantValues.Auth.UseSSL).Value);
                    int connectionTimeout = int.Parse(LDAPConfigSection.GetSection(ConstantValues.Auth.ConnectionTimeout).Value);

                    string userDn = $"{userName}@{domainName}";
                    try
                    {
                        using (var connection = new LdapConnection
                        {
                            ConnectionTimeout = connectionTimeout,
                            SecureSocketLayer = useSSL
                        })
                        {
                            await connection.ConnectAsync(domainHost, domainPort);
                            await connection.BindAsync(userDn, password);
                            if (connection.Bound)
                                userExistsOnActiveDirectory = true;
                        }
                    }
                    catch (LdapException ex)
                    {
                    }
                }

                if (userExistsOnActiveDirectory)
                {
                    var encryptedUsername = AESEncryptionHelper.EncryptString(userName);
                    User? ADUser = userManager.Users.AsEnumerable()
                        .Where(u => u.ActiveDirectoryName != null
                        && u.ActiveDirectoryName.ToLower() == encryptedUsername.ToLower())
                        .FirstOrDefault();
                    if (ADUser != null)
                    {
                        List<ClaimModel> claims = GetAllUserClaims(ADUser.Id);
                        string token = GetJWTToken(ADUser, claims);
                        string refreshToken = GetJWTRefreshToken(ADUser);
                        return ((claims, token, refreshToken), ConstantValues.ServicesHttpResponses.OK);
                    }
                    else
                    {
                        User newADUser = new User
                        {
                            UserName = encryptedUsername,
                            ActiveDirectoryName = encryptedUsername
                        };
                        IdentityResult result = await userManager.CreateAsync(newADUser);
                        if (result.Succeeded)
                        {
                            //await userManager.AddToRoleAsync(newADUser, ConstantValues.Roles.User);
                            List<ClaimModel> claims = GetAllUserClaims(newADUser.Id);
                            string token = GetJWTToken(newADUser, claims);
                            string refreshToken = GetJWTRefreshToken(newADUser);
                            return ((claims, token, refreshToken), ConstantValues.ServicesHttpResponses.OK);
                        }
                        else
                        {
                            string toSender = "";
                            foreach (IdentityError error in result.Errors)
                                toSender += error.Description + "\n";
                            return (null, ConstantValues.ServicesHttpResponses.InternalServerError);
                        }
                    }
                }
                else
                {
                    try
                    {
                        var encryptedUsername = AESEncryptionHelper.EncryptString(userName);
                        User user = await userManager.FindByNameAsync(encryptedUsername);
                        if (user != null)
                        {
                            var passwordHasher = new PasswordHasher<string>();
                            var loginResult = passwordHasher
                                .VerifyHashedPassword(user.UserName == null ? string.Empty : user.UserName,
                                user.PasswordHash == null ? string.Empty : user.PasswordHash
                                , password);
                            /*l'algoritmo di login è cambiato tra login 6 e 8, quindi le vecchie password restituiscono
                             SuccessRehashNeeded perché bisognerebbe rihasharle con l'algoritmo nuovo, ma è pur sempre
                             un success.. a quanto pare però, l'8 è retrocompatibile con il 6*/
                            if (loginResult == PasswordVerificationResult.Success
                                || loginResult == PasswordVerificationResult.SuccessRehashNeeded)
                            {
                                List<ClaimModel> claims = GetAllUserClaims(user.Id);
                                string token = GetJWTToken(user, claims);
                                string refreshToken = GetJWTRefreshToken(user);
                                return ((claims, token, refreshToken), ConstantValues.ServicesHttpResponses.OK);
                            }
                            else
                                return (null, ConstantValues.ServicesHttpResponses.LoginFailed);
                        }
                        else
                            return (null, ConstantValues.ServicesHttpResponses.UserDoesntExist);
                    }
                    catch (Exception ex)
                    {
                        return (null, (ex, ConstantValues.ServicesHttpResponses.InternalServerError.StatusCode));
                    }
                }
            }
            catch (Exception ex)
            {
                return (null, (ex, ConstantValues.ServicesHttpResponses.InternalServerError.StatusCode));
            }
        }

        public async Task<((List<ClaimModel> Claims, string Token, string RefreshToken) LoginResult,
            (object StatusCodeMessage, HttpStatusCode StatusCode) StatusCodeResult)>
            RefreshJWTTokens(string refreshToken)
        {
            SecurityToken? validatedToken = null;
            var tokenHandler = new JwtSecurityTokenHandler();

            IPrincipal principal = tokenHandler.ValidateToken(refreshToken, ConstantValues.Auth.TokenValidationParameters, out validatedToken);

            string? userId = ((JwtSecurityToken)validatedToken).Subject;
            User user = await userManager.FindByIdAsync(userId);
            List<ClaimModel> claims = GetAllUserClaims(userId);
            string newToken = GetJWTToken(user, claims);
            string newRefreshToken = GetJWTRefreshToken(user);
            return ((claims, newToken, newRefreshToken), ConstantValues.ServicesHttpResponses.OK);
        }

        private string GetJWTToken(User user, List<ClaimModel> userClaims)
        {
            var issuer = configuration
                .GetSection(ConstantValues.Auth.AuthSettings)
                .GetSection(ConstantValues.Auth.JWT)
                .GetSection(ConstantValues.Auth.JWTIssuer).Value;
            var audience = configuration
                .GetSection(ConstantValues.Auth.AuthSettings)
                .GetSection(ConstantValues.Auth.JWT)
                .GetSection(ConstantValues.Auth.JWTAudience).Value;
            var tokenExpiry = configuration
                .GetSection(ConstantValues.Auth.AuthSettings)
                .GetSection(ConstantValues.Auth.JWT)
                .GetSection(ConstantValues.Auth.JWTExpiry).Value;
            var encryptionKey = Encoding.ASCII.GetBytes(configuration
                .GetSection(ConstantValues.Auth.AuthSettings)
                .GetSection(ConstantValues.Auth.JWT)
                .GetSection(ConstantValues.Auth.JWTEncryptionKey).Value);
            var signingKey = Encoding.ASCII.GetBytes(configuration
                .GetSection(ConstantValues.Auth.AuthSettings)
                .GetSection(ConstantValues.Auth.JWT)
                .GetSection(ConstantValues.Auth.JWTSigningKey).Value);

            var encryptingCredentials = new EncryptingCredentials(new SymmetricSecurityKey(encryptionKey), JwtConstants.DirectKeyUseAlg, SecurityAlgorithms.Aes256CbcHmacSha512);

            /*nel token metto prima dei claim di sistema e user id, poi quelli
             specifici di ciascun utente*/
            List<Claim> claims = new List<Claim>()
            {
                new Claim(ConstantValues.Auth.SecurityStampClaimName, user.SecurityStamp),
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Iss, issuer),
                new Claim(JwtRegisteredClaimNames.Aud, audience),
                new Claim(JwtRegisteredClaimNames.Exp, tokenExpiry),
                new Claim(JwtRegisteredClaimNames.Iat, DateTime.Now.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            foreach (ClaimModel claimModel in userClaims)
                claims.Add(new Claim(claimModel.ClaimType, claimModel.ClaimValue));

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                //EncryptingCredentials = new EncryptingCredentials(new System.Security.Cryptography.X509Certificates.X509Certificate2(
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(int.Parse(tokenExpiry)),
                Issuer = issuer,
                Audience = audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(signingKey), SecurityAlgorithms.HmacSha512Signature),
                EncryptingCredentials = encryptingCredentials
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);
            string tokenString = tokenHandler.WriteToken(token);

            return tokenString;
        }

        private string GetJWTRefreshToken(User user)
        {
            var issuer = configuration
                .GetSection(ConstantValues.Auth.AuthSettings)
                .GetSection(ConstantValues.Auth.JWT)
                .GetSection(ConstantValues.Auth.JWTIssuer).Value;
            var audience = configuration
                .GetSection(ConstantValues.Auth.AuthSettings)
                .GetSection(ConstantValues.Auth.JWT)
                .GetSection(ConstantValues.Auth.JWTAudience).Value;
            var refreshTokenExpiry = configuration
                .GetSection(ConstantValues.Auth.AuthSettings)
                .GetSection(ConstantValues.Auth.JWT)
                .GetSection(ConstantValues.Auth.JWTRefreshTokenExpiry).Value;
            var encryptionKey = Encoding.ASCII.GetBytes(configuration
                .GetSection(ConstantValues.Auth.AuthSettings)
                .GetSection(ConstantValues.Auth.JWT)
                .GetSection(ConstantValues.Auth.JWTEncryptionKey).Value);
            var signingKey = Encoding.ASCII.GetBytes(configuration
                .GetSection(ConstantValues.Auth.AuthSettings)
                .GetSection(ConstantValues.Auth.JWT)
                .GetSection(ConstantValues.Auth.JWTSigningKey).Value);

            var encryptingCredentials = new EncryptingCredentials(new SymmetricSecurityKey(encryptionKey), JwtConstants.DirectKeyUseAlg, SecurityAlgorithms.Aes256CbcHmacSha512);

            /*nel refresh token metto solo i claim di sistema e user id*/
            List<Claim> claims = new List<Claim>()
            {
                new Claim(ConstantValues.Auth.SecurityStampClaimName, user.SecurityStamp),
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Iss, issuer),
                new Claim(JwtRegisteredClaimNames.Aud, audience),
                new Claim(JwtRegisteredClaimNames.Exp, refreshTokenExpiry),
                new Claim(JwtRegisteredClaimNames.Iat, DateTime.Now.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                //EncryptingCredentials = new EncryptingCredentials(new System.Security.Cryptography.X509Certificates.X509Certificate2(
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(int.Parse(refreshTokenExpiry)),
                Issuer = issuer,
                Audience = audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(signingKey), SecurityAlgorithms.HmacSha512Signature),
                EncryptingCredentials = encryptingCredentials
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);
            string tokenString = tokenHandler.WriteToken(token);

            return tokenString;
        }

        public async Task<(object Result, HttpStatusCode StatusCode)> Logout(ClaimsPrincipal User)
        {
            try
            {
                // :)
                //if (signInManager.IsSignedIn(User))
                //{
                //    await signInManager.SignOutAsync();
                //}
                return ConstantValues.ServicesHttpResponses.OK;
            }
            catch (Exception ex)
            {
                return (ex, HttpStatusCode.InternalServerError);
            }
        }

        public (object Result, HttpStatusCode StatusCode) GetUsersWithFilter(string? userName, string? email)
        {
            var users = authDBContext.Users.ToList();
            for (int i = 0; i < users.Count; i++)
            {
                users[i].Email = AESEncryptionHelper.DecryptString(users[i].Email);
                users[i].UserName = AESEncryptionHelper.DecryptString(users[i].UserName);
                users[i].ActiveDirectoryName = AESEncryptionHelper.DecryptString(users[i].ActiveDirectoryName);
            }
            users = users.Where(u =>
             (string.IsNullOrEmpty(userName) ? u.UserName != null : (u.UserName != null && u.UserName.ToLower().Contains(userName.ToLower())))
             &&
             (string.IsNullOrEmpty(email) ? true : (u.Email != null && u.Email.ToLower().Contains(email.ToLower())))
            ).Select(u => new User()
            {
                Id = u.Id,
                Email = u.Email,
                UserName = u.UserName,
                UserRoles = (from r in authDBContext.Roles
                             join ur in authDBContext.UserRoles.Where(ur => ur.UserId == u.Id) on r.Id equals ur.RoleId
                             select new Role()
                             {
                                 ConcurrencyStamp = r.ConcurrencyStamp,
                                 Id = r.Id,
                                 Name = r.Name,
                                 NormalizedName = r.NormalizedName,
                                 RoleClaims = authDBContext.RoleClaims.Where(rc => rc.RoleId == r.Id).ToList()
                             }).ToList(),
                UserClaims = authDBContext.UserClaims.Where(uc => uc.UserId == u.Id).ToList()
            }).OrderBy(u => u.UserName).ToList();

            return (users, HttpStatusCode.OK);
        }

        public async Task<(object Result, HttpStatusCode StatusCode)> InsertUser(string userName, string email, string password)
        {
            var encryptedUsername = AESEncryptionHelper.EncryptString(userName);
            var encryptedEmail = AESEncryptionHelper.EncryptString(email);
            User user = await userManager.FindByEmailAsync(encryptedEmail);
            if (user == null)
            {
                user = new User
                {
                    UserName = encryptedUsername,
                    Email = encryptedEmail
                };
                IdentityResult result = await userManager.CreateAsync(user, password);
                if (result.Succeeded)
                    return ConstantValues.ServicesHttpResponses.OK;
                return (result.Errors, HttpStatusCode.InternalServerError);
            }
            else
                return ("Email is already taken", HttpStatusCode.BadRequest);
        }

        public async Task<(object Result, HttpStatusCode StatusCode)> DeleteUser(string userId)
        {
            User user = await userManager.FindByIdAsync(userId);
            if (user != null)
            {
                List<IdentityUserClaim<string>> userClaims = authDBContext.UserClaims.Where(uc => uc.UserId == user.Id).ToList();
                authDBContext.UserClaims.RemoveRange(userClaims);

                List<IdentityUserRole<string>> userRoles = authDBContext.UserRoles.Where(ur => ur.UserId == user.Id).ToList();
                authDBContext.UserRoles.RemoveRange(userRoles);

                await authDBContext.SaveChangesAsync();

                IdentityResult result = await userManager.DeleteAsync(user);

                if (result.Succeeded)
                    return ConstantValues.ServicesHttpResponses.OK;
                return (result.Errors, HttpStatusCode.InternalServerError);
            }
            return ConstantValues.ServicesHttpResponses.BadRequest;
        }

        public async Task<(object Result, HttpStatusCode StatusCode)> AddRoleToUser(string userID, string roleName)
        {
            User user = await userManager.FindByIdAsync(userID);
            await userManager.AddToRoleAsync(user, roleName);
            await userManager.UpdateSecurityStampAsync(user);
            return ConstantValues.ServicesHttpResponses.OK;
        }
        public async Task<(object Result, HttpStatusCode StatusCode)> RemoveRoleFromUser(string userID, string roleName)
        {
            User user = await userManager.FindByIdAsync(userID);
            await userManager.RemoveFromRoleAsync(user, roleName);
            await userManager.UpdateSecurityStampAsync(user);
            return ConstantValues.ServicesHttpResponses.OK;
        }
        public async Task<(object Result, HttpStatusCode StatusCode)> AddClaimToUser(string userID, string claimType, string claimValue)
        {
            if (!string.IsNullOrEmpty(claimType) && !string.IsNullOrEmpty(claimValue))
            {
                authDBContext.UserClaims.Add(new IdentityUserClaim<string>()
                {
                    UserId = userID,
                    ClaimType = claimType,
                    ClaimValue = claimValue
                });
                await authDBContext.SaveChangesAsync();

                User user = await userManager.FindByIdAsync(userID);
                await userManager.UpdateSecurityStampAsync(user);
                return ConstantValues.ServicesHttpResponses.OK;
            }
            else
                return ("Values cant be null", HttpStatusCode.BadRequest);
        }
        public async Task<(object Result, HttpStatusCode StatusCode)> RemoveClaimFromUser(string userID, string claimType, string claimValue)
        {
            List<IdentityUserClaim<string>> claims = authDBContext.UserClaims
                .Where(uc => uc.UserId == userID && uc.ClaimType == claimType && uc.ClaimValue == claimValue).ToList();
            if (claims.Count > 0)
            {
                foreach (IdentityUserClaim<string> claim in claims)
                    authDBContext.UserClaims.Remove(claim);
                await authDBContext.SaveChangesAsync();

                User user = await userManager.FindByIdAsync(userID);
                await userManager.UpdateSecurityStampAsync(user);
            }
            return ConstantValues.ServicesHttpResponses.OK;
        }

        public async Task<(object Result, HttpStatusCode StatusCode)> GetAvailableClaimsForUser(string userID)
        {
            List<ClaimModel> allBackendClaims = GetAllBackendClaims();
            List<ClaimModel> userClaims = GetAllUserClaims(userID);

            var resultClaims = createList(new { HasClaim = false, Claim = new ClaimModel(string.Empty, string.Empty) });
            List<T> createList<T>(T type) => new List<T>();
            foreach (var claim in userClaims)
                resultClaims.Add(new { HasClaim = true, Claim = claim });
            foreach (var claim in allBackendClaims)
                if (!resultClaims.Any(c => c.Claim.ClaimType == claim.ClaimType && c.Claim.ClaimValue == claim.ClaimValue))
                    resultClaims.Add(new { HasClaim = false, Claim = claim });
            return (resultClaims.OrderByDescending(r => r.HasClaim).ThenBy(r => r.Claim.ClaimType).ToList(), HttpStatusCode.OK);
        }

        public async Task<(object Result, HttpStatusCode StatusCode)> GetAvailableRolesForUser(string userID)
        {
            var userRoles = (from r in authDBContext.Roles
                             join ur in authDBContext.UserRoles.Where(ur => ur.UserId == userID) on r.Id equals ur.RoleId
                             select new Role()
                             {
                                 ConcurrencyStamp = r.ConcurrencyStamp,
                                 Id = r.Id,
                                 Name = r.Name,
                                 NormalizedName = r.NormalizedName,
                                 RoleClaims = authDBContext.RoleClaims.Where(rc => rc.RoleId == r.Id).ToList()
                             }).ToList();
            var allRoles = authDBContext.Roles.ToList();

            var resultRoles = createList(new { HasRole = false, Role = new Role() });
            List<T> createList<T>(T type) => new List<T>();
            foreach (var role in userRoles)
                resultRoles.Add(new { HasRole = true, Role = role });
            foreach (var role in allRoles)
                if (!resultRoles.Any(r => r.Role.Name == role.Name))
                    resultRoles.Add(new { HasRole = false, Role = role });
            return (resultRoles.OrderByDescending(r => r.HasRole).ThenBy(r => r.Role.Name).ToList(), HttpStatusCode.OK);
        }

        public async Task<(object Result, HttpStatusCode StatusCode)> UpdateUser(string userId, string? userName, string? email, string? password)
        {
            User? user = await authDBContext.Users.Where(u => u.Id == userId).FirstOrDefaultAsync();
            string response = string.Empty;
            if (user != null)
            {
                if (!string.IsNullOrEmpty(userName))
                {
                    var encryptedUsername = AESEncryptionHelper.EncryptString(userName);
                    if (await authDBContext.Users.Where(u => u.UserName == encryptedUsername).FirstOrDefaultAsync() == null)
                    {
                        IdentityResult result = await userManager.SetUserNameAsync(user, encryptedUsername);
                        if (result.Succeeded)
                            response += "The UserName has been changed. The user has been automatically logged out" + "\n";
                        else
                            foreach (IdentityError error in result.Errors)
                                response += error.Code + ": " + error.Description + "\n";
                    }
                    else
                        response += "Username " + userName + " is already taken" + "\n";
                }
                if (!string.IsNullOrEmpty(email))
                {
                    var encryptedEmail = AESEncryptionHelper.EncryptString(email);
                    if (await authDBContext.Users.Where(u => u.Email == encryptedEmail).FirstOrDefaultAsync() == null)
                    {
                        IdentityResult result = await userManager.SetEmailAsync(user, encryptedEmail);
                        if (result.Succeeded)
                            response += "The email has been changed. The user has been automatically logged out" + "\n";
                        else
                            foreach (IdentityError error in result.Errors)
                                response += error.Code + ": " + error.Description + "\n";
                    }
                    else
                        response += "Email " + email + " is already taken" + "\n";
                }
                if (!string.IsNullOrEmpty(password))
                {
                    string token = await userManager.GeneratePasswordResetTokenAsync(user);
                    IdentityResult result = await userManager.ResetPasswordAsync(user, token, password);
                    if (result.Succeeded)
                    {
                        await userManager.UpdateSecurityStampAsync(user);
                        response += "The password has been changed" + "\n";
                    }
                    else
                        foreach (IdentityError error in result.Errors)
                            response += error.Code + ": " + error.Description + "\n";
                }
                if (string.IsNullOrEmpty(email) && string.IsNullOrEmpty(password))
                    response += "Nothing to do" + "\n";
            }
            return (response, HttpStatusCode.OK);
        }

        public (object Result, HttpStatusCode StatusCode) GetRolesWithFilter(string roleName)
        {
            var roles = authDBContext.Roles.Where(r =>
            string.IsNullOrEmpty(roleName) ? r.Name != null : r.Name.ToLower().Contains(roleName.ToLower()))
                .Select(r => new Role()
                {
                    ConcurrencyStamp = r.ConcurrencyStamp,
                    Id = r.Id,
                    Name = r.Name,
                    NormalizedName = r.NormalizedName,
                    RoleClaims = authDBContext.RoleClaims.Where(rc => rc.RoleId == r.Id).OrderBy(rc => rc.ClaimType).ToList()
                }).OrderBy(r => r.Name).ToList();
            return (roles, HttpStatusCode.OK);
        }

        public async Task<(object Result, HttpStatusCode StatusCode)> InsertRole(string roleName)
        {
            if (!string.IsNullOrEmpty(roleName))
            {
                await this.roleManager.CreateAsync(new Role()
                {
                    Name = roleName
                });

                //authDBContext.Roles.Add(new Role()
                //{
                //    Name = roleName,
                //    NormalizedName = roleName.ToUpperInvariant()
                //});
                //await authDBContext.SaveChangesAsync();
                return ConstantValues.ServicesHttpResponses.OK;
            }
            else
                return ConstantValues.ServicesHttpResponses.BadRequest;
        }
        public async Task<(object Result, HttpStatusCode StatusCode)> UpdateRole(string roleID, string roleName)
        {
            if (!string.IsNullOrEmpty(roleName))
            {
                Role? role = authDBContext.Roles.Where(r => r.Id == roleID).FirstOrDefault();
                if (role != null)
                {
                    role.Name = roleName;
                    await this.roleManager.UpdateAsync(role);

                    List<User> users = (from ur in authDBContext.UserRoles
                                        join u in authDBContext.Users on ur.UserId equals u.Id
                                        where ur.RoleId == roleID
                                        select u).ToList();
                    foreach (User user in users)
                        await userManager.UpdateSecurityStampAsync(user);

                    return ConstantValues.ServicesHttpResponses.OK;
                }
                else
                    return ConstantValues.ServicesHttpResponses.BadRequest;
            }
            else
                return ConstantValues.ServicesHttpResponses.BadRequest;
        }
        public async Task<(object Result, HttpStatusCode StatusCode)> DeleteRole(string roleID)
        {
            Role? role = authDBContext.Roles.Where(r => r.Id == roleID).FirstOrDefault();
            if (role != null)
            {
                List<IdentityRoleClaim<string>> claims = authDBContext.RoleClaims.Where(uc => uc.RoleId == role.Id).ToList();
                authDBContext.RoleClaims.RemoveRange(claims);

                List<IdentityUserRole<string>> userRoles = authDBContext.UserRoles.Where(ur => ur.RoleId == role.Id).ToList();
                authDBContext.UserRoles.RemoveRange(userRoles);

                List<User> users = (from ur in authDBContext.UserRoles
                                    join u in authDBContext.Users on ur.UserId equals u.Id
                                    where ur.RoleId == roleID
                                    select u).ToList();
                foreach (User user in users)
                    await userManager.UpdateSecurityStampAsync(user);

                authDBContext.Roles.Remove(role);
                await authDBContext.SaveChangesAsync();
                return ConstantValues.ServicesHttpResponses.OK;
            }
            return ConstantValues.ServicesHttpResponses.BadRequest;
        }
        public async Task<(object Result, HttpStatusCode StatusCode)> AddClaimToRole(string roleID, string claimType, string claimValue)
        {
            if (!string.IsNullOrEmpty(claimType) && !string.IsNullOrEmpty(claimValue))
            {
                authDBContext.RoleClaims.Add(new IdentityRoleClaim<string>()
                {
                    RoleId = roleID,
                    ClaimType = claimType,
                    ClaimValue = claimValue
                });
                await authDBContext.SaveChangesAsync();

                List<User> users = (from ur in authDBContext.UserRoles
                                    join u in authDBContext.Users on ur.UserId equals u.Id
                                    where ur.RoleId == roleID
                                    select u).ToList();
                foreach (User user in users)
                    await userManager.UpdateSecurityStampAsync(user);
                return ConstantValues.ServicesHttpResponses.OK;
            }
            else
                return ("Values cant be null", HttpStatusCode.BadRequest);
        }
        public async Task<(object Result, HttpStatusCode StatusCode)> RemoveClaimFromRole(string roleID, string claimType, string claimValue)
        {
            List<IdentityRoleClaim<string>> claims = authDBContext.RoleClaims.Where(uc => uc.RoleId == roleID && uc.ClaimType == claimType && uc.ClaimValue == claimValue).ToList();
            if (claims.Count > 0)
            {
                foreach (IdentityRoleClaim<string> claim in claims)
                    authDBContext.RoleClaims.Remove(claim);
                await authDBContext.SaveChangesAsync();

                List<User> users = (from ur in authDBContext.UserRoles
                                    join u in authDBContext.Users on ur.UserId equals u.Id
                                    where ur.RoleId == roleID
                                    select u).ToList();
                foreach (User user in users)
                    await userManager.UpdateSecurityStampAsync(user);
                return ConstantValues.ServicesHttpResponses.OK;
            }
            return ConstantValues.ServicesHttpResponses.BadRequest;
        }

        public List<ClaimModel> GetAllBackendClaims()
        {
            List<ClaimModel> claims = new List<ClaimModel>();
            foreach (var claim in typeof(ConstantValues.Auth.Claims.Types).GetFields())
                claims.Add(new ClaimModel(claim.Name, ConstantValues.Auth.Claims.Values.True));
            return claims;
        }

        public async Task<(object Result, HttpStatusCode StatusCode)> GetAvailableClaimsForRole(string roleID)
        {
            var allBackendClaims = GetAllBackendClaims();
            var allBackendClaimsAsRoleClaims = new List<IdentityRoleClaim<string>>();
            foreach (var claim in allBackendClaims)
                allBackendClaimsAsRoleClaims.Add(new IdentityRoleClaim<string>() { ClaimType = claim.ClaimType, ClaimValue = claim.ClaimValue });

            List<IdentityRoleClaim<string>> allDatabaseClaims = authDBContext.RoleClaims.ToList();
            List<IdentityRoleClaim<string>> roleClaims = allDatabaseClaims.Where(uc => uc.RoleId == roleID).ToList();

            var resultClaims = createList(new { HasClaim = false, Claim = new IdentityRoleClaim<string>() });
            List<T> createList<T>(T type) => new List<T>();
            foreach (var claim in roleClaims)
                resultClaims.Add(new { HasClaim = true, Claim = claim });
            foreach (var claim in allDatabaseClaims)
                if (!resultClaims.Any(c => c.Claim.ClaimType == claim.ClaimType && c.Claim.ClaimValue == claim.ClaimValue))
                    resultClaims.Add(new { HasClaim = false, Claim = claim });
            foreach (var claim in allBackendClaimsAsRoleClaims)
                if (!resultClaims.Any(c => c.Claim.ClaimType == claim.ClaimType && c.Claim.ClaimValue == claim.ClaimValue))
                    resultClaims.Add(new { HasClaim = false, Claim = claim });
            return (resultClaims.OrderByDescending(r => r.HasClaim).ThenBy(r => r.Claim.ClaimType).ToList(), HttpStatusCode.OK);
        }

        public List<ClaimModel> GetAllUserClaims(string userId)
        {
            User? user = authDBContext.Users.Where(u => u.Id == userId).Select(u => new User()
            {
                Id = u.Id,
                UserRoles = (from r in authDBContext.Roles
                             join ur in authDBContext.UserRoles.Where(ur => ur.UserId == u.Id) on r.Id equals ur.RoleId
                             select new Role()
                             {
                                 ConcurrencyStamp = r.ConcurrencyStamp,
                                 Id = r.Id,
                                 Name = r.Name,
                                 NormalizedName = r.NormalizedName,
                                 RoleClaims = authDBContext.RoleClaims.Where(rc => rc.RoleId == r.Id).ToList()
                             }).ToList(),
                UserClaims = authDBContext.UserClaims.Where(uc => uc.UserId == u.Id).ToList()
            }).ToList().FirstOrDefault(); /*se si fa solo FirstOrDefault senza ToList Entity Framework
                                           non riesce a tradurla.. sia in SQL server che postgres :C*/
            List<ClaimModel> claims = new List<ClaimModel>();
            foreach (var claim in user.UserClaims)
                if (!claims.Any(c => c.ClaimType == claim.ClaimType && c.ClaimValue == claim.ClaimValue))
                    claims.Add(new ClaimModel(claim.ClaimType, claim.ClaimValue));
            foreach (var role in user.UserRoles)
                foreach (var claim in role.RoleClaims)
                    if (!claims.Any(c => c.ClaimType == claim.ClaimType && c.ClaimValue == claim.ClaimValue))
                        claims.Add(new ClaimModel(claim.ClaimType, claim.ClaimValue));

            return claims;
        }
    }
}