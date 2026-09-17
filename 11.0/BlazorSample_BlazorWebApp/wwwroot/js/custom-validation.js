function registerCustomValidators(blazor) {
  blazor.formValidation.addValidator('startswith', (context) => {
    const value = context.value;

    if (!value) {
      return { success: true };
    }

    return { success: value.startsWith(context.params.prefix) };
  });
}

// Automatic startup initializes formValidation before this script runs.
// Manual startup calls registerCustomValidators after Blazor.start completes.
if (Blazor.formValidation) {
  registerCustomValidators(Blazor);
}
