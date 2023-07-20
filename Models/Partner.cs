using Azure;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection.Metadata;

namespace DeliveryTerminal.Models
{
    [DisplayName("Контрагент")]
    public class Partner
    {
        public long Id { get; set; }
        [DisplayName("Название")]
        public string Name { get; set; }
        [DisplayName("ИНН")]
        public string TaxID { get; set; }
        [DisplayName("ОГРН")]
        public string? RegID { get; set; }
        [DisplayName("Поставщик")]
        public bool IsSupplier { get; set; }
        [DisplayName("Заказчик")]
        public bool IsCustomer { get; set; }
        [DisplayName("Электронная почта")]
        public string? Email { get; set; }
        [DisplayName("Юридический адрес")]
        public string Address { get; set; }
        [DisplayName("Контактное лицо")]
        public string ContactName { get; set; }
        [DisplayName("Контактный телефон")]
        public string ContactPhone { get; set; }
        [DisplayName("Нужна экспедиторская выписка")]
        public bool NeedExp { get; set; }
        [Display(Name = "Назначенный регион доставки")]
        public long? RegionId { get; set; }
        public virtual Region? Region { get; set; }
        [Display(Name = "Пользователь заказчика")]
        public string? UserId { get; set; }
        public virtual IdentityUser? User { get; set; }
        [Display(Name = "Назначенная транспортная компания")]
        public long? TransportingCompanyId { get; set; }
        public virtual TransportingCompany? TransportingCompany { get; set; }
        public string? ColumnsFilter { get; set; }
        public Tariff? Tariff { get; set; }
        [DisplayName("Заказчики, назначенные мне")]
        public virtual List<PartnerAssignment> PartnersAssigned { get; set; } = new();
        [DisplayName("Поставщики, кому назначен я")]
        public virtual List<PartnerAssignment> PartnersAssignedTo { get; set; } = new();
        [DisplayName("Заказчики, назначенные мне")]
        public virtual List<Registry> RegistryAsReceiver { get; set; } = new();
        [DisplayName("Поставщики, кому назначен я")]
        public virtual List<Registry> RegistryAsSender { get; set; } = new();
        [DisplayName("Заказчики, назначенные мне")]
        public virtual List<Return> ReturnsAsClient { get; set; } = new();
        [DisplayName("Поставщики, кому назначен я")]
        public virtual List<Return> ReturnsAsSupplier { get; set; } = new();

    }
}
