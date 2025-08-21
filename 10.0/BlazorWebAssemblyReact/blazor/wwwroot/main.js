import Blazor from './_framework/blazor.webassembly.js';

let isBlazorLoaded = false;

export async function start() {
    if (isBlazorLoaded) {
        return;
    }

    await Blazor.start();
    isBlazorLoaded = true;
}
