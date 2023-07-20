namespace DeliveryTerminal.Models
{
    public class HistoryEntry
    {
        public string UserName { get; set; }
        public DateTime DateTime { get; set; }
        public string Reason { get; set; }
        public Dictionary<string, object> OldValues { get; set; }
        public Dictionary<string, object> NewValues { get; set; }
    }
    public class HistoryViewModel
    {
        public string? Error { get; set; }
        public string Table { get; set; }
        public string Id { get; set; }
        public Dictionary<string,string> Columns { get; set; }
        public List<HistoryEntry> HistoryEntries { get; set; } = new List<HistoryEntry>();

    }
}
