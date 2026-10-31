using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace x1_lavorazione_pratiche.DB.Entities.AuthEntities
{
    public class Role : IdentityRole
    {
        [NotMapped]
        public List<IdentityRoleClaim<string>> RoleClaims { get; set; } = new List<IdentityRoleClaim<string>>();
    }
}