using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace DeliveryTerminal.Models
{
    public class TariffViewModel
    {
        public long IdVM { get; set; }
        public string? PartnerNameVM { get; set; }
        [Column(TypeName = "decimal(18, 2)"), DisplayName("Тариф (%)")]
        public decimal TaficcCross { get; set; } = 0;
        [Column(TypeName = "decimal(18, 2)"), DisplayName("Тариф (руб/кг)")]
        public decimal TaficcExp { get; set; } = 0;
        [Column(TypeName = "decimal(18, 2)"), DisplayName("Надбавка за сбор")]
        public decimal Addition { get; set; } = 0;
        [DisplayName("Обоснование изменений")]
        public string? EditReason { get; set; }
    }
}
