using System;
using System.ComponentModel.DataAnnotations;

namespace sistema_gestione_pratiche.DB.Entities
{
    public class Notification
    {
        [Key]
        public Guid Id { get; set; }
        public Guid SenderId { get; set; }
        public Guid RecipientId { get; set; }
        public string? Message { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime? ReadDate { get; set; }
    }
}