using sistema_gestione_pratiche.DB.Entities.AuthEntities;
using System.Collections.Generic;

namespace sistema_gestione_pratiche.Models.AuthModels
{
    public class UsersAndRolesModel
    {
        public List<User> Users { get; set; } = new List<User>();
        public List<Role> Roles { get; set; } = new List<Role>();
        public string? UserID { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public string? ConfirmPassword { get; set; }
        public string? ClaimType { get; set; }
        public string? ClaimValue { get; set; }
        public string? RoleID { get; set; }
        public string? RoleName { get; set; }
    }
}