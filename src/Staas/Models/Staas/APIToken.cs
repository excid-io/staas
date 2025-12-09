using System.ComponentModel.DataAnnotations;

namespace Excid.Staas.Models
{
    public class APIToken
    {
        [Key]
        public int Id { get; set; }
        public string Signer { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
    }
}
