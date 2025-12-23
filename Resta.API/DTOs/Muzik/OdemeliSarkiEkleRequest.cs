using System.ComponentModel.DataAnnotations;

namespace Resta.API.DTOs.Muzik
{
    public class OdemeliSarkiEkleRequest : SarkiEkleRequest
    {
        [Required(ErrorMessage = "Ödeme miktarı gereklidir")]
        [Range(2, int.MaxValue, ErrorMessage = "Ödeme en az 2 TL olmalıdır")]
        public int odemeMiktari { get; set; }

        [Required(ErrorMessage = "Ödeme referansı gereklidir")]
        public string odemeReferans { get; set; } = null!;
    }
}