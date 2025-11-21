// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.InteropServices.JavaScript;
using System.Runtime.Versioning;
using QRCoder;

// https://github.com/codebude/QRCoder

[SupportedOSPlatform("browser")]
public partial class QRGenerator
{
    private static readonly int MAX_QR_SIZE = 20;

    [JSExport]
    internal static byte[] Generate(string text, int qrSize)
    {
        if (qrSize >= MAX_QR_SIZE)
        {
            throw new Exception($"QR code size must be less than {MAX_QR_SIZE}. Try again.");
        }

        QRCodeGenerator qrGenerator = new();
        QRCodeData qrCodeData = qrGenerator.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q);
        BitmapByteQRCode qrCode = new(qrCodeData);

        return qrCode.GetGraphic(qrSize);
    }
}
