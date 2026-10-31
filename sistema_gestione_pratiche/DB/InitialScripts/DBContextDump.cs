using Microsoft.Extensions.DependencyInjection;
using System;
using sistema_gestione_pratiche.DB.Contexts;
using sistema_gestione_pratiche.Helpers;
using sistema_gestione_pratiche.Services;
using sistema_gestione_pratiche.DB.Entities.AuthEntities;
using System.Collections.Generic;
using System.Linq;

namespace sistema_gestione_pratiche.DB.InitialScripts
{
    public static class DBContextDump
    {
        public static void InitialDump(IServiceProvider serviceProvider)
        {
            using (IServiceScope scope = serviceProvider.CreateScope())
            {
                //DBContext dBContext = scope.ServiceProvider.GetRequiredService<DBContext>();
                //for (int i = 0; i < 20000; i++)
                //{
                //    dBContext.Persons.Add(
                //        new Person()
                //        {
                //            Age = i,
                //            BirthDay = DateTime.Now,
                //            FirstName = AESEncryptionHelper.EncryptString(("first" + i)),
                //            LastName = AESEncryptionHelper.EncryptString(("last" + i))
                //        }
                //        );
                //}
                //dBContext.SaveChanges();
                AuthDBContext authDBContext = scope.ServiceProvider.GetService<AuthDBContext>();
                AuthService authService = scope.ServiceProvider.GetService<AuthService>();
                var roleExists = authDBContext.Roles.Where(r => r.Name == "Admin").Any();
                var claimExists = false;
                if (!roleExists)
                {
                    var roleCreated = authService.InsertRole("Admin").Result;
                    if (roleCreated.StatusCode == System.Net.HttpStatusCode.OK)
                        roleExists = true;
                }
                if (roleExists)
                {
                    Role adminRole = ((List<Role>)authService.GetRolesWithFilter("Admin").Result)[0];

                    if (adminRole.RoleClaims
                        .Where(c => c.ClaimType == "CANDOANYTHING" && c.ClaimValue == "True")
                        .Count() < 1)
                    {
                        var claimCreated = authService.AddClaimToRole(adminRole.Id, "CANDOANYTHING", "True").Result;
                        if (claimCreated.StatusCode == System.Net.HttpStatusCode.OK)
                            claimExists = true;
                    }
                    else
                        claimExists = true;
                }
                if (roleExists && claimExists)
                {
                    var adminUserExists = authDBContext.Users
                     .Any(u => u.UserName == AESEncryptionHelper.EncryptString("admin", null));

                    if (!adminUserExists)
                    {
                        var userCreated = authService.InsertUser("admin", "admin@admin.com", "Password01!").Result;
                        //var adminUser = authService.GetUsersWithFilter("admin", null).Result;
                        var adminUser = authDBContext.Users
                            .Where(u => u.UserName == AESEncryptionHelper.EncryptString("admin", null)).FirstOrDefault();
                        if (adminUser != null)
                        {
                            var roleAddedToUser = authService.AddRoleToUser(adminUser.Id, "Admin").Result;
                        }
                    }
                }
            }
        }
    }
}
