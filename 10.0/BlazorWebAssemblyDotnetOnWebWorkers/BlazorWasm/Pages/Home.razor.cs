// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.AspNetCore.Components;
using System.Runtime.Versioning;
using Components;

namespace Pages;

[SupportedOSPlatform("browser")]
public partial class Home : ComponentBase
{
    private string imageUrl = string.Empty;
    private string? text;
    private int size = 5;
    private Popup popup = new();

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await Client.InitClient();
    }

    private async Task GenerateQR()
    {
        try
        {
            if (text is not null)
            {
                imageUrl = await Client.GenerateQR(text, size);
            }
        }
        catch(Exception ex)
        {
            imageUrl = string.Empty;
            popup.Show(title: "Error", message: ex.Message);
        }

        await InvokeAsync(StateHasChanged);
    }
}
