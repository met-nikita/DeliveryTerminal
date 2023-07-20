using System.ComponentModel;

namespace DeliveryTerminal.Models
{
    public class PartnerAssignment
    {
        public long Id { get; set; }
        [DisplayName("Поставщик")]
        public long PartnerId { get; set; }
        public virtual Partner Partner { get; set; }
        [DisplayName("Грузополучатель")]
        public long AssignedPartnerId { get; set; }
        public virtual Partner AssignedPartner { get; set; }
        [DisplayName("Кросс-докинг")]
        public bool CrossDock { get; set; } = true;
        [DisplayName("Стандартные поставки")]
        public bool Expeditor { get; set; } = false;

    }
}
