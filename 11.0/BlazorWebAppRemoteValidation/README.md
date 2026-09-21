# Blazor Web App remote validation

This sample uses an Interactive WebAssembly form and a Minimal API in the Blazor Web App host to demonstrate remote form validation.

The form:

1. Runs data annotations validation locally.
1. Sends locally valid input to the Minimal API.
1. Displays field-keyed errors returned by the endpoint.
1. Proceeds only when local and remote validation succeed.

The endpoint validates data annotations before its handler runs and applies a private business rule in the handler.

WebAssembly prerendering is disabled so the form's `HttpClient` dependency only needs to be registered in the `.Client` project.

Run the sample from the solution directory:

```dotnetcli
dotnet run --project BlazorWebAppRemoteValidation
```
