using System;

namespace x1_lavorazione_pratiche.Models
{
    public class PraticaModel
    {
        public Guid? Id { get; set; }
        public Guid? CreatorId { get; set; }
        public string? CreatorName { get; set; }
        public string? CreatorLastName { get; set; }
        public string? CreatorCF { get; set; }
        public DateTime? CreatorDateOfBirth { get; set; }
        public string? FileName { get; set; }
        public byte[]? ContentBytes { get; set; }
        public string? Status { get; set; }
        public DateTime? CreationDate { get; set; }
    }
}
