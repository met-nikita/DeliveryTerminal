using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DeliveryTerminal.Models
{
    [DisplayName("Тариф")]
    public class Tariff
    {
        public long Id { get; set; }
        public long PartnerId { get; set; }
        public virtual Partner Partner { get; set; }
        [Column(TypeName = "decimal(18, 2)"), Display(Name="Тариф (%)")]
        public decimal TaficcCross { get; set; } = 0;
        [Column(TypeName = "decimal(18, 2)"), Display(Name="Тариф (руб/кг)")]
        public decimal TaficcExp { get; set; } = 0;
        [Column(TypeName = "decimal(18, 2)"), Display(Name="Надбавка за сбор")]
        public decimal Addition { get; set; } = 0;
    }
}
