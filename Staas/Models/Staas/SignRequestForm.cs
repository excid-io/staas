using System.ComponentModel.DataAnnotations;

namespace Excid.Staas.Models
{
    public class SignRequestForm
    {
        [Required(ErrorMessage = "Please select a file")]
        [Display(Name = "Select a file to sign")]
        public required IFormFile Data { get; set; }

        [Display(Name = "Comment")]
        public string? Comment { get; set; }
    }
}
