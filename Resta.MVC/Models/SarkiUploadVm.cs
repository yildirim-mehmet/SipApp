namespace Resta.MVC.Models
{
    public class SarkiUploadVm_
    {
        public string Ad { get; set; } = null!;
        public int Sure { get; set; }
        public bool Aktif { get; set; } = true;

        public IFormFile Dosya { get; set; } = null!;
    }

}
