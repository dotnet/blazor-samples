# Standalone Blazor WebAssembly Entra Groups and Roles Sample App

Sample name: `BlazorWebAssemblyEntraGroupsAndRoles`

This sample app demonstrates how to configure Blazor WebAssembly to use Microsoft Entra ID (ME-ID) groups and roles.

For more information, see [Microsoft Entra (ME-ID) groups, Administrator Roles, and App Roles](https://learn.microsoft.com/aspnet/core/blazor/security/webassembly/microsoft-entra-id-groups-and-roles).

## Steps to run the sample

1. Clone this repository or download a ZIP archive of the repository. For more information, see [How to download a sample](https://learn.microsoft.com/aspnet/core/introduction-to-aspnet-core#how-to-download-a-sample).

1. In the `appsettings.json` file, provide values for the following placeholders from the app's ME-ID registration in the Azure portal:

   * `{TENANT ID}`: The Directory (Tenant) Id GUID value.
   * `{CLIENT ID}`: The Application (Client) Id GUID value.

1. Confirm that the app's registration has *delegated* MS Graph `User.Read`, `RoleManagement.Read.Directory`, and `AdministrativeUnit.Read.All` permissions in the **API Permissions** pane. The last two permissions require admin consent.
  
1. Run the app. When testing locally, we recommend using a new in-private/incognito browser session for each test to prevent lingering cookies from interfering with tests. For more information, see the *Troubleshoot* section of [Secure an ASP.NET Core Blazor WebAssembly standalone app with Microsoft Entra ID](https://learn.microsoft.com/aspnet/core/blazor/security/webassembly/standalone-with-microsoft-entra-id#troubleshoot).

1. Log in with a test user account that has:

   * At least one custom app role assigned.
   * At least one built-in ME-ID Administrator Role assignment.
   * At least one custom security/O365 group assignment.
   * Optional: An administrative unit assignment.

1. Visit the app's User Claims page to see:

   * App role claims appear in "`role`" claims.
   * Built-in Azure Administrator role assignments appear in "`directoryRole`" claims.
   * Security/O365 groups appear in "`directoryGroup`" claims.
   * Administrative units appear in "`administrativeUnit`" claims.

1. At this point, you can use the approaches in the [Authorization configuration](https://learn.microsoft.com/aspnet/core/blazor/security/webassembly/microsoft-entra-id-groups-and-roles#authorization-configuration) and [App Roles](https://learn.microsoft.com/aspnet/core/blazor/security/webassembly/microsoft-entra-id-groups-and-roles#app-roles) sections of the article create policy-based and role-based access to Razor components based on the user's roles, ME-ID built-in roles, groups, and administrative units.
