using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DeliveryTerminal.Controllers;

namespace DeliveryTerminal.Models
{
    public class PartnerViewModel
    {
        public long IdVM { get; set; }
        [Required(ErrorMessage = "Поле '{0}' обязательно к заполнению")]
        [DisplayName("Название")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Поле '{0}' обязательно к заполнению")]
        [DisplayName("ИНН")]
        public string TaxID { get; set; }
        [Required(ErrorMessage = "Поле '{0}' обязательно к заполнению")]
        [DisplayName("ОГРН")]
        public string? RegID { get; set; }
        [DisplayName("Поставщик")]
        public bool IsSupplier { get; set; }
        [DisplayName("Заказчик")]
        public bool IsCustomer { get; set; }
        [DisplayName("Электронная почта")]
        public string? Email { get; set; }
        [Required(ErrorMessage = "Поле '{0}' обязательно к заполнению")]
        [DisplayName("Юридический адрес")]
        public string Address { get; set; }
        [Required(ErrorMessage = "Поле '{0}' обязательно к заполнению")]
        [DisplayName("Контактное лицо")]
        public string ContactName { get; set; }
        [Required(ErrorMessage = "Поле '{0}' обязательно к заполнению")]
        [DisplayName("Контактный телефон")]
        public string ContactPhone { get; set; }
        [DisplayName("Нужна экспедиторская выписка")]
        public bool NeedExp { get; set; }
        [Display(Name = "Назначенный регион доставки")]
        public long? RegionId { get; set; }
        [Display(Name = "Назначенная транспортная компания")]
        public long? TransportingCompanyId { get; set; }
        [Display(Name = "Пользователь заказчика")]
        public string? UserId { get; set; }
        [Column(TypeName = "decimal(18, 2)"), Display(Name = "Тариф (%)")]
        public decimal TaficcCross { get; set; } = 0;
        [Column(TypeName = "decimal(18, 2)"), Display(Name = "Тариф (руб/кг)")]
        public decimal TaficcExp { get; set; } = 0;
        [Column(TypeName = "decimal(18, 2)"), Display(Name = "Надбавка за сбор")]
        public decimal Addition { get; set; } = 0;
        public Dictionary<long, String> AllPartnersNames { get; set; } = new();
        public Dictionary<long, bool> AllPartnersSelection { get; set; } = new();
    }
}
