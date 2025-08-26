using QRCoder;
using System;
using System.Runtime.InteropServices.JavaScript;

[assembly:System.Runtime.Versioning.SupportedOSPlatform("browser")]

public partial class QR
{
    [JSExport]
    internal static string Generate(string text, int pixelsPerBlock)
    {
        QRCodeGenerator qrGenerator = new QRCodeGenerator();
        QRCodeData qrCodeData = qrGenerator.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q);

        BitmapByteQRCode qrCode = new BitmapByteQRCode(qrCodeData);
        return Convert.ToBase64String(qrCode.GetGraphic(pixelsPerBlock));
    }
}
