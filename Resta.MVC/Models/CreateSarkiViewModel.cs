using System.ComponentModel.DataAnnotations;

namespace Resta.MVC.Models
{
    public class CreateSarkiViewModel
    {
        [Required(ErrorMessage = "Şarkı adı gereklidir")]
        [MaxLength(200)]
        public string Ad { get; set; } = null!;

        [Required(ErrorMessage = "Dosya yolu gereklidir")]
        [MaxLength(500)]
        public string DosyaYolu { get; set; } = null!;

        [Required(ErrorMessage = "Süre gereklidir")]
        [Range(1, 3600)]
        public int Sure { get; set; }

        public bool Aktif { get; set; } = true;
    }
}