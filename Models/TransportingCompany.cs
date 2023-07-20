using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DeliveryTerminal.Models
{
    [DisplayName("Транспортная компания")]
    public class TransportingCompany
    {
        public long Id { get; set; }
        [DisplayName("Название")]
        public string Name { get; set; }
        [DisplayName("ИНН")]
        public string TaxID { get; set; }
        [DisplayName("ОГРНИП")]
        public string RegID { get; set; }
        [DisplayName("Название банка")]
        public string BankName { get; set; }
        [DisplayName("Р/С")]
        public string RS { get; set; }
        [DisplayName("К/С")]
        public string KS { get; set; }
        [DisplayName("БИК")]
        public string BIK { get; set; }
        [DisplayName("По-умолчанию")]
        public bool DefaultTC { get; set; }
    }
}
