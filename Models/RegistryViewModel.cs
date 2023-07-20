using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using DeliveryTerminal.Models.Enums;

namespace DeliveryTerminal.Models
{
    public class RegistryViewModel
    {
        public long IdVM { get; set; }
        [Display(Name = "Тип поставки")]
        public DeliveryType DeliveryType { get; set; } = DeliveryType.Expeditor;
        [Display(Name = "Грузополучатель")]
        public long ReceiverId { get; set; }
        public virtual Partner? Receiver { get; set; }
        [Display(Name = "Дата поступления"), DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        [DataType(DataType.Date)]
        public DateTime ReceiveDate { get; set; }
        [Display(Name = "Время поступления")]
        [DataType(DataType.Time), DisplayFormat(DataFormatString = "{0:HH:mm}")]
        public DateTime ReceiveTime { get; set; }
        [Display(Name = "Поставщик")]
        public long SenderId { get; set; }
        public virtual Partner? Sender { get; set; }
        [Display(Name = "Регион")]
        public long RegionId { get; set; }
        public virtual Region? Region { get; set; }
        [Display(Name = "Кол-во паллет")]
        public int? CountPallets { get; set; } = 0;
        [Display(Name = "Кол-во коробок")]
        public int? CountBoxes { get; set; } = 0;
        [Display(Name = "Кол-во негабаритных")]
        public int? CountOversized { get; set; } = 0;
        [Display(Name = "Вес")]
        public int? Weight { get; set; }
        [Display(Name = "Место сбора")]
        public PackagingLoc PackagingLoc { get; set; } = PackagingLoc.Warehouse;
        [Display(Name = "Водитель")]
        public string? Driver { get; set; } = "";
        [Display(Name = "Учет паллет")]
        public int? OwnPallets { get; set; } = 0;
        [Display(Name = "Комментарии")]
        public string? Notes { get; set; } = "";
        [Display(Name = "№ УПД")]
        public string UPDID { get; set; } = "";
        [Display(Name = "Дата УПД")]
        [DataType(DataType.Date), DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime UPDDate { get; set; }
        [Column(TypeName = "decimal(18, 2)"), Display(Name = "Сумма УПД")]
        public decimal UPDSum { get; set; } = 0;
        [Display(Name = "№ ЭР")]
        public string? ExpID { get; set; } = "";
        [Display(Name = "Дата ЭР")]
        [DataType(DataType.Date), DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime? ExpDate { get; set; }
        [Column(TypeName = "decimal(18, 2)"), Display(Name = "Ставка")]
        public decimal? Rate { get; set; }
        [DisplayName("Обоснование изменений")]
        public string? EditReason { get; set; }
    }
}
