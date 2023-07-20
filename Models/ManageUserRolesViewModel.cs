using System.ComponentModel.DataAnnotations;

namespace DeliveryTerminal.Models
{
    public class ManageUserRolesEntry
    {
        public string RoleId { get; set; }
        public string RoleName { get; set; }
        public bool Selected { get; set; }
    }
    public class ManageUserRolesViewModel
    {
        [EmailAddress]
        [Required(ErrorMessage = "Поле '{0}' обязательно к заполнению")]
        [Display(Name ="E-mail")]
        public string Email { get; set; }
        [DataType(DataType.Password)]
        [StringLength(100, ErrorMessage = "{0} должен состоять как минимум из {2} и как максимум из {1} символов.", MinimumLength = 6)]
        [Display(Name = "Пароль")]
        public string? Password { get; set; }
        public List<ManageUserRolesEntry> Roles { get; set; } = new List<ManageUserRolesEntry>();
    }
    public class CreateUserViewModel
    {
        [EmailAddress]
        [Required(ErrorMessage = "Поле '{0}' обязательно к заполнению")]
        [Display(Name = "E-mail")]
        public string Email { get; set; }
        [DataType(DataType.Password)]
        [Required(ErrorMessage = "Поле '{0}' обязательно к заполнению")]
        [StringLength(100, ErrorMessage = "{0} должен состоять как минимум из {2} и как максимум из {1} символов.", MinimumLength = 6)]
        [Display(Name = "Пароль")]
        public string Password { get; set; }
        public List<ManageUserRolesEntry> Roles { get; set; } = new List<ManageUserRolesEntry>();
    }
}
