# `BlazorWebAssemblyXrefGenerator`

Sample app to accompany [ASP.NET Core Blazor Host and Deploy: GitHub Pages](https://learn.microsoft.com/aspnet/core/blazor/host-and-deploy/webassembly#github-pages).

The Xref Generator tool is used by ASP.NET Core documentation authors to format API document links for ASP.NET Core article markdown.

This site is automatically deployed to GitHub Pages by a [GitHub Action script (`static.yml`)](https://github.com/dotnet/blazor-samples/blob/main/.github/workflows/static.yml).

GitHub settings:

* **Actions** > **General**
  * **Actions permissions** > **Allow all actions and reusable workflows**
  * **Workflow permissions**
    * **Read repository contents and packages permissions**
    * **Allow GitHub Actions to create and approve pull requests**: Enabled (selected)
* **Pages** > **Build and deployment**
  * **Source** > **GitHub Actions**
  * Selected workflow: **Static HTML** and use the [`static.yml` file](https://github.com/dotnet/blazor-samples/blob/main/.github/workflows/static.yml) for this site. Configure the following entries in the script for your deployment:
    * Publish directory (`PUBLISH_DIR`)
    * .NET SDK version (`dotnet-version` via the [`actions/setup-dotnet` Action](https://github.com/actions/setup-dotnet))
    * Push path (`on:push:paths`)
    * Publish path (`dotnet publish` command)
    * Base HREF (`base_href` for the [`SteveSandersonMS/ghaction-rewrite-base-href` Action](https://github.com/SteveSandersonMS/ghaction-rewrite-base-href))
  * **Custom domain**: Set if you intend to use a custom domain.
  * **Enforce HTTPS**> Enabled (selected)

The GitHub-hosted Ubuntu (latest) server has a version of the .NET SDK installed. You can remove the [`actions/setup-dotnet` Action](https://github.com/actions/setup-dotnet) script step that installs the .NET SDK if the SDK pre-installed with the GitHub image is sufficient to compile the app. To determine the .NET SDK installed for `ubuntu-latest`:

1. Go to the [**Available Images** section of the `actions/runner-images` GitHub repository](https://github.com/actions/runner-images?tab=readme-ov-file#available-images).
1. Locate the `ubuntu-latest` image, which is the first table row.
1. Select the link in the `Included Software` column.
1. Scroll down to the [*.NET Tools* section](https://github.com/actions/runner-images/blob/main/images/ubuntu/Ubuntu2404-Readme.md#net-tools) to see the .NET Core SDK installed with the image.
