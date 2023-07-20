using System.ComponentModel.DataAnnotations;

namespace DeliveryTerminal.Models
{
    public class SupplierReturn
    {
        public long Id { get; set; }
        public long SupplierId { get; set; }
        public Partner Supplier { get; set; }
        public int WarningCount { get; set; }
        public DateTime LastWarning { get; set; }
    }
}
