using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DeliveryTerminal.Models
{
    [Display(Name="Возврат")]
    public class Return
    {
        public long Id { get; set; }
        [Display(Name = "Клиент")]
        public long ClientId { get; set; }
        public virtual Partner? Client { get; set; }
        [Display(Name = "Поставщик")]
        public long SupplierId { get; set; }
        public virtual Partner? Supplier { get; set; }
        [Display(Name = "Дата отправки"), DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        [DataType(DataType.Date)]
        public DateTime SendDate { get; set; }
        [Display(Name = "Количество мест")]
        public int Count { get; set; }
        [Display(Name = "Комментарии")]
        public string? Notes { get; set; }
        [Display(Name = "Дата поступления на терминал"), DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        [DataType(DataType.Date)]
        public DateTime? ReceiveDate { get; set; }
        [Display(Name = "Дата передачи товара представителю"), DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        [DataType(DataType.Date)]
        public DateTime? ReturnDate { get; set; }
        [Display(Name = "ФИО представителя")]
        public string? RepName { get; set; }
    }
}
