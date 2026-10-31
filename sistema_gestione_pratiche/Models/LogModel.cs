namespace sistema_gestione_pratiche.Models
{
    public class LogModel
    {
        public string[]? Additional { get; set; }
        public int? ColumnNumber { get; set; }
        public string? FileName { get; set; }
        public int? Level { get; set; }
        public int? LineNumber { get; set; }
        public string? Message { get; set; }
        public string? TimeStamp { get; set; }
    }
}
