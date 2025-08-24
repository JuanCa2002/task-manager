using System.ComponentModel.DataAnnotations;

namespace TaskManager.Models.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "ErrorRequried")]
        [EmailAddress(ErrorMessage = "ErrorEmail")]
        [Display(Name = "EmailField")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "ErrorRequried")]
        [DataType(DataType.Password)]
        [Display(Name = "PasswordField")]
        public string Password { get; set; } = string.Empty;
    }
}
