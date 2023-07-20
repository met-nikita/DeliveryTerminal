using System.ComponentModel;

namespace DeliveryTerminal.Models
{
    public class PartnerAssignmentViewModelEntry
    {
        public long IdVM { get; set; }
        public long IdAsVM { get; set; }
        [DisplayName("Грузополучатель")]
        public string? NameVM { get; set; }
        [DisplayName("Кросс-докинг")]
        public bool CrossDock { get; set; }
        [DisplayName("Экспедиторские")]
        public bool Expeditor { get; set; }
    }
    public class PartnerAssignmentViewModel
    {
        public long IdVM { get; set; }
        public string? PartnerNameVM { get; set; }
        public List<PartnerAssignmentViewModelEntry> Entries { get; set; } = new List<PartnerAssignmentViewModelEntry>();
    }
}
