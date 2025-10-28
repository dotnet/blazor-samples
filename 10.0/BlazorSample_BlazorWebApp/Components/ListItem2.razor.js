export function assignDotNetHelper(element, dotNetHelper) {
  element.dotNetHelper = dotNetHelper;
}

export async function interopCall(element) {
  await element.dotNetHelper.invokeMethodAsync('UpdateMessage');
}
