// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

import { dotnet } from './_framework/dotnet.js'

let exportsPromise = null;

async function createRuntimeAndGetExports() {
    const { getAssemblyExports, getConfig } = await dotnet.create();
    const config = getConfig();
    return await getAssemblyExports(config.mainAssemblyName);
}

export async function generate(text, pixelsPerBlock) {
    if (!exportsPromise) {
        exportsPromise = createRuntimeAndGetExports();
    }

    const exports = await exportsPromise;
    return exports.QR.Generate(text, pixelsPerBlock);
}
