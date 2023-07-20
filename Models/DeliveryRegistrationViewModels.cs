using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DeliveryTerminal.Models.Enums;

namespace DeliveryTerminal.Models
{
    public class NameCrossDockExpeditor
    {
        public string Name { get; set; }
        public bool CrossDock { get; set; }
        public bool Expeditor { get; set; }
    }
    public class DeliveryRegistrationReceiverSelectViewModel
    {
        public string Error { get; set; }
        public long IdS { get; set; }
        public string NameS { get; set; }
        public Dictionary<long, NameCrossDockExpeditor> AssignedPartnersNames { get; set; } = new();
    }
    public class DeliveryRegistrationViewModel
    {
        public string? Error { get; set; }
        public long IdS { get; set; }
        public long IdR { get; set; }
        public string? NameS { get; set; }
        public string? NameR { get; set; }
        public DeliveryType DeliveryType { get; set; }
        [Display(Name = "Кол-во паллет")]
        public int? CountPallets { get; set; }
        [Display(Name = "Кол-во коробок")]
        public int? CountBoxes { get; set; }
        [Display(Name = "Кол-во негабаритных")]
        public int? CountOversized { get; set; }
        [DisplayName("Вес")]
        public int? Weight { get; set; }
        [Display(Name = "Место сбора")]
        public PackagingLoc PackagingLoc { get; set; } = PackagingLoc.Warehouse;
        [Display(Name = "Регион доставки")]
        public long RegionId { get; set; }
        [Display(Name = "Водитель")]
        public string? Driver { get; set; } = "";
        [Display(Name = "Учет паллет")]
        public int? OwnPallets { get; set; }
        [Display(Name = "Комментарии")]
        public string? Notes { get; set; } = "";
        [Required(ErrorMessage = "Поле '{0}' обязательно к заполнению")]
        [Display(Name = "№ УПД")]
        public string UPDID { get; set; } = "";
        [Display(Name = "Дата УПД")]
        [DataType(DataType.Date), DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime UPDDate { get; set; } = DateTime.Now;
        [Column(TypeName = "decimal(18, 2)"), DisplayName("Сумма УПД")]
        public decimal UPDSum { get; set; }
        [DisplayName("Требуется ЭР")]
        public bool ExpRequired { get; set; }
        public bool LockExpRequired { get; set; }
        public List<int>? Weights { get; set; } = new();
    }
    public class PrintDocumentsPrintEntry
    {
        public string Name { get; set; }
        public int? Weight { get; set; }
        public string Text { get; set; }
    }
    public class PrintDocumentsViewModel
    {
        public List<PrintDocumentsPrintEntry> Entries { get; set; }
        public Registry Registry { get; set; }
        public String PackageName { get; set; }
        public int PackageSlots { get; set; }
        public TransportingCompany TransportingCompany { get; set; }
    }

    public class ReturnAcceptViewModel
    {
        public long SupplierId { get; set; }
        public bool? CanSkip { get; set; }
        public List<Return>? Returns { get; set; }
        [Display(Name ="ФИО представителя")]
        public string RepName { get; set; }
    }
}
