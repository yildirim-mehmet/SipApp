using System.ComponentModel.DataAnnotations;

namespace Resta.API.DTOs.Muzik
{
    public class SarkiEkleRequest
    {
        [Required(ErrorMessage = "Şarkı ID gereklidir")]
        [Range(1, int.MaxValue, ErrorMessage = "Geçersiz şarkı ID")]
        public int sarkiId { get; set; }

        [Required(ErrorMessage = "Masa ID gereklidir")]
        [Range(1, int.MaxValue, ErrorMessage = "Geçersiz masa ID")]
        public int masaId { get; set; }
    }
}