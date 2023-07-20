using System.ComponentModel.DataAnnotations;

namespace DeliveryTerminal.Models
{
    public class ReturnViewModel
    {
        [Display(Name = "Клиент")]
        public long ClientId { get; set; }
        [Display(Name = "Поставщик")]
        public long SupplierId { get; set; }
        [Display(Name = "Дата отправки"), DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        [DataType(DataType.Date)]
        public DateTime SendDate { get; set; } = DateTime.Today;
        [Display(Name = "Количество мест")]
        public int Count { get; set; }
        [Display(Name = "Комментарии")]
        public string? Notes { get; set; }
    }
}
