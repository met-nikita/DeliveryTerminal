using System.ComponentModel;

namespace DeliveryTerminal.Models
{
    [DisplayName("Регион")]
    public class Region
    {
        public long Id { get; set; }
        [DisplayName("Название")]
        public string Name { get; set; }
        [DisplayName("Доставки")]
        public virtual List<Registry> Registry { get; set; } = new();
    }
}
