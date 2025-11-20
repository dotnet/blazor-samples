// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

import { dotnet } from '../_framework/dotnet.js'

let assemblyExports = null;
let startupError = undefined;

try {
    const { getAssemblyExports, getConfig } = await dotnet.create();
    const config = getConfig();
    assemblyExports = await getAssemblyExports(config.mainAssemblyName);
}
catch (err) {
    startupError = err.message;
}

self.addEventListener('message', async function (e) {
    try {
        if (!assemblyExports) {
            throw new Error(startupError || "worker exports not loaded");
        }
        let result = null;
        switch (e.data.command) {
            case "generateQR":
                result = assemblyExports.QRGenerator.Generate(e.data.text, e.data.size);
                break;
            default:
                throw new Error("Unknown command: " + e.data.command);
        }
        self.postMessage({
            command: "response",
            requestId: e.data.requestId,
            result,
        });
    }
    catch (err) {
        self.postMessage({
            command: "response",
            requestId: e.data.requestId,
            error: err.message,
        });
    }
}, false);
