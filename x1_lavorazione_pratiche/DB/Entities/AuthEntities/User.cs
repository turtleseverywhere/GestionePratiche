using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace x1_lavorazione_pratiche.DB.Entities.AuthEntities
{
    public class User : IdentityUser
    {
        public string? ActiveDirectoryName { get; set; }

        [NotMapped]
        public List<Role> UserRoles { get; set; } = new List<Role>();
        [NotMapped]
        public List<IdentityUserClaim<string>> UserClaims { get; set; } = new List<IdentityUserClaim<string>>();
    }
}