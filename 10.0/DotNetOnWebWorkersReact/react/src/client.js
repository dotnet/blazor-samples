// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

const pendingRequests = {};
let pendingRequestId = 0;

const dotnetWorker = new Worker('../../qr/wwwroot/worker.js', { type: "module" } );
dotnetWorker.addEventListener('message', async function (e) {
    switch (e.data.command) {
        case "response":
            if (!e.data.requestId) {
                console.error("No requestId in response from worker");
            }
            const request = pendingRequests[e.data.requestId];
            delete pendingRequests[e.data.requestId];
            if (e.data.error) {
                request.reject(new Error(e.data.error));
            }
            request.resolve(e.data.result);
            break;
        default:
            console.log('Worker said: ', e.data);
            break;
    }
}, false);

function sendRequestToWorker(request) {
    pendingRequestId++;
    const promise = new Promise((resolve, reject) => {
        pendingRequests[pendingRequestId] = { resolve, reject };
    });
    dotnetWorker.postMessage({
        ...request,
        requestId: pendingRequestId
    });
    return promise;
}

export async function generateQR(text, size) {
    const response = await sendRequestToWorker({
        command: "generateQR",
        text: text,
        size: size
    });
    const blob = new Blob([response], { type: 'image/png' });
    return URL.createObjectURL(blob);
}
