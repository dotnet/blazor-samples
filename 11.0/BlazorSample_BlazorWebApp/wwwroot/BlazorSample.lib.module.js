// A JavaScript initializer works with automatic or manual Blazor startup. The validation service
// is available when afterWebStarted runs, even if the current page contains no validated form.
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
