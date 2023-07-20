using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel;

namespace DeliveryTerminal.Models
{
    public class QRCodeGenViewModel
    {
        [DisplayName("Поставщик")]
        public List<Partner> Senders { get; set; }
        [DisplayName("Грузополучатель")]
        public List<Partner> Receivers { get; set; }
        public long Sender { get; set; }
        public long Receiver { get; set; }
        public string Svg { get; set; }
        public string SenderName { get; set; }
        public string ReceiverName { get; set; }
    }
}
