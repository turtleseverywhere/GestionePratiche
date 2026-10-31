namespace sistema_gestione_pratiche.Models
{
    public class SignalRMessageModel
    {
        public object? Message { get; set; }
        public string? UserId { get; set; }
        public string? ClientId { get; set; }
        public string? ConnectionId { get; set; }
    }
}
