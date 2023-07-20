using System.ComponentModel.DataAnnotations;

namespace DeliveryTerminal.Models
{
    public class Audit
    {
        public long Id { get; set; }
        [Display(Name ="Пользователь")]
        public string? UserId { get; set; }
        [Display(Name = "Тип операции")]
        public string Type { get; set; }
        [Display(Name = "Название таблицы")]
        public string TableName { get; set; }
        [Display(Name = "Время")]
        public DateTime DateTime { get; set; }
        [Display(Name = "Старые значения")]
        public string? OldValues { get; set; }
        [Display(Name = "Новые значения")]
        public string? NewValues { get; set; }
        [Display(Name = "Затронутые столбцы")]
        public string? AffectedColumns { get; set; }
        [Display(Name = "Идентификатор записи")]
        public string? PrimaryKey { get; set; }
        [Display(Name = "Обоснование")]
        public string? Reason { get; set; }
    }
}
