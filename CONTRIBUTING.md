# Contribute to the ASP.NET Core documentation

This document covers the process for contributing Blazor sample apps for the [ASP.NET documentation site](https://learn.microsoft.com/aspnet/).

## How to make a simple correction or suggestion

Simple changes are made in the browser by selecting the **Edit** link in the upper-right corner of the browser window. (In a narrow browser window, expand the **options** bar to see the **Edit** link.) Follow the directions to create a pull request (PR). We will review the PR and accept it or suggest changes.

## How to make a more complex submission

You need a basic understanding of [Git and GitHub.com](https://guides.github.com/activities/hello-world/).

* Open an [issue](https://github.com/dotnet/blazor-samples/issues) describing what you want to do. Wait for approval from the team before you invest much time.
* Fork the [`dotnet/blazor-samples` GitHub repository](https://github.com/dotnet/blazor-samples) and create a branch for your changes.
* Submit a PR to the *main* branch with your changes.
* If your PR has the label 'cla-required' assigned, [complete the Contribution License Agreement (CLA)](https://cla.dotnetfoundation.org/).
* Respond to PR feedback.

## Code compilation checks

The **Snippets 5000** workflow runs for pull requests that target `main`. It uses [Snippets5000](https://github.com/dotnet/docs-tools/tree/main/snippets5000) to discover and compile sample projects and solutions affected by changes to code files, Razor files, project files, solution files, or build configuration files such as `global.json` and `snippets.5000.json`.

Review compilation errors in the workflow logs and fix failures in the affected samples before merging. The workflow runs on Windows with the .NET 11 preview SDK and the .NET 10 SDK and runtime.

To rerun the check manually, select **Actions** > **Snippets 5000** > **Run workflow** and enter the pull request number. The workflow checks out that pull request's merge ref and builds its affected samples.

## Related content

* [Contribute to the ASP.NET Core documentation](https://github.com/dotnet/AspNetCore.Docs/blob/main/CONTRIBUTING.md)
* [Funamentals: Razor components](https://learn.microsoft.com/aspnet/core/blazor/fundamentals/#razor-components)
* [ASP.NET Core Razor components: Razor syntax](https://learn.microsoft.com/aspnet/core/blazor/components/#razor-syntax)
