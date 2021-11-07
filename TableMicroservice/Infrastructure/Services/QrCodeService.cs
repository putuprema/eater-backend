using QRCoder;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Processing;
using System.Drawing.Imaging;

namespace Infrastructure.Services
{
    public class QrCodeService : IQrCodeService
    {
        private readonly IStorageService _storageService;
        private readonly FontCollection _fonts = new();
        private readonly AppConfig _appConfig;

        public QrCodeService(IStorageService storageService, IOptions<AppConfig> appConfig)
        {
            _storageService = storageService;
            _appConfig = appConfig.Value;

            foreach (var fontFileName in Directory.GetFiles(Path.Combine(_appConfig.ApplicationRootPath, "Resources", "Fonts")))
            {
                _fonts.Install(fontFileName);
            }
        }

        public async Task<string> GenerateQrStickerAsync(Table table, CancellationToken cancellationToken = default)
        {
            using var finalImageStream = new MemoryStream();

            using (var finalImage = await Image.LoadAsync(Path.Combine(_appConfig.ApplicationRootPath, "Resources", "qr-sticker-template.png"), cancellationToken))
            {
                #region Create QR Code Logic
                var qrCodeData = new QRCodeGenerator().CreateQrCode(table.Id, QRCodeGenerator.ECCLevel.H);
                var qrBitmap = new QRCode(qrCodeData).GetGraphic(20);

                using var qrBitmapStream = new MemoryStream();
                qrBitmap.Save(qrBitmapStream, ImageFormat.Png);
                qrBitmapStream.Seek(0, SeekOrigin.Begin);

                using var qrImage = await Image.LoadAsync(qrBitmapStream);
                qrImage.Mutate(x => x.Resize(new Size(300, 300)));
                #endregion

                #region Generate QR Sticker
                var qrCodeLocation = new Point(x: (finalImage.Width / 2) - (qrImage.Width / 2), y: (finalImage.Height / 2) - (qrImage.Height / 2) + 12);

                finalImage.Mutate(context => context
                    .DrawImage(qrImage, qrCodeLocation, 1f)
                    .DrawText(
                        new DrawingOptions
                        {
                            TextOptions = new TextOptions
                            {
                                HorizontalAlignment = HorizontalAlignment.Center
                            }
                        },
                        $"Table {table.Number}",
                        _fonts.Find("Nunito").CreateFont(35, FontStyle.Bold),
                        Color.White,
                        new Point(finalImage.Width / 2, qrCodeLocation.Y + 340))
                );

                await finalImage.SaveAsync(finalImageStream, new PngEncoder(), cancellationToken);
                #endregion
            }

            finalImageStream.Seek(0, SeekOrigin.Begin);
            return await _storageService.UploadQrStickerAsync(finalImageStream, $"{table.Id}.png", cancellationToken);
        }
    }
}
