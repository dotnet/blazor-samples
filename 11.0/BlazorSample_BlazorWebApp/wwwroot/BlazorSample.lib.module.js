// JavaScript initializer for the app. The Blazor.formValidation service is created while
// Blazor starts, so custom client-side validators are registered from afterWebStarted.
export function afterWebStarted(blazor) {
  blazor.formValidation.addValidator('startswith', (context) => {
    const value = context.value;

    // An empty value is valid. Use [Required] to require a value.
    if (!value) {
      return { success: true };
    }

    return { success: value.startsWith(context.params.prefix) };
  });
}
