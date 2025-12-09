using System.ComponentModel.DataAnnotations;

namespace Excid.Staas.Models
{
    public class SignRequestForm
    {
        [Required(ErrorMessage = "Please select a file")]
        [Display(Name = "Select a file to sign")]
        public string HashBase64 { get; set; } = string.Empty;

        [Display(Name = "Comment")]
        public string Comment { get; set; } = string.Empty;
    }
}
