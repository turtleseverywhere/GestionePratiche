namespace sistema_gestione_pratiche.Models.AuthModels
{
    public class ClaimModel
    {
        public string? ClaimType { get; set; }
        public string? ClaimValue { get; set; }

        public ClaimModel() { }
        public ClaimModel(string type, string value)
        {
            this.ClaimType = type;
            this.ClaimValue = value;
        }
    }
}
