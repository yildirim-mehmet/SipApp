using QRCoder;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
//using static System.Net.Mime.MediaTypeNames;

namespace Resta.API.Helpers
{
    public static class QrHelper
    {
        public static byte[] GenerateQrPng(string text, int size = 512)
        {
            QRCodeGenerator generator = new QRCodeGenerator();
            QRCodeData data = generator.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q);
            var qr = new PngByteQRCode(data);
            byte[] qrBytes = qr.GetGraphic(20);

            using var image = Image.Load<Rgba32>(qrBytes);
            image.Mutate(x => x.Resize(size, size));

            using var ms = new MemoryStream();
            image.Save(ms, new PngEncoder());
            return ms.ToArray();
        }
    }
}
