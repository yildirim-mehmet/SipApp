using System.ComponentModel.DataAnnotations;

namespace Resta.API.DTOs.Muzik
{
    public class CreateSarkiRequest
    {
        [Required(ErrorMessage = "Şarkı adı gereklidir")]
        [MaxLength(200, ErrorMessage = "Şarkı adı en fazla 200 karakter olabilir")]
        public string ad { get; set; } = null!;

        [Required(ErrorMessage = "Dosya yolu gereklidir")]
        [MaxLength(500, ErrorMessage = "Dosya yolu en fazla 500 karakter olabilir")]
        public string dosyaYolu { get; set; } = null!;

        [Required(ErrorMessage = "Süre gereklidir")]
        [Range(1, 3600, ErrorMessage = "Süre 1-3600 saniye arasında olmalıdır")]
        public int sure { get; set; }

        public bool aktif { get; set; } = true;
    }
}