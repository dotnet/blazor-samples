// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.InteropServices.JavaScript;
using System.Runtime.Versioning;

[SupportedOSPlatform("browser")]
public partial class Client
{
    private static bool workerDotnetStarted;

    public static async Task InitClient()
    {
        if (workerDotnetStarted)
        {
            return;
        }

        workerDotnetStarted = true;

        await JSHost.ImportAsync(
            moduleName: nameof(Client),
            moduleUrl: $"../Clients/Client.razor.js");
    }

    [JSImport("generateQR", nameof(Client))]
    public static partial Task<string> GenerateQR(string text, int size);
}
