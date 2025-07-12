# `BlazorWebAssemblyXrefGenerator`

Sample app to accompany [ASP.NET Core Blazor Host and Deploy: GitHub Pages](https://learn.microsoft.com/aspnet/core/blazor/host-and-deploy/webassembly/github-pages).

The Xref Generator tool is used by ASP.NET Core documentation authors to format API document links for ASP.NET Core article markdown.

This site is automatically deployed to GitHub Pages by a [GitHub Action script (`static.yml`)](https://github.com/dotnet/blazor-samples/blob/main/.github/workflows/static.yml).

## GitHub settings

* **Actions** > **General**
  * **Actions permissions**
    * **Allow enterprise actions, and select non-enterprise, actions and reusable workflows**
    * Select **Allow actions created by GitHub**
    * **Allow actions and reusable workflows** > `stevesandersonms/ghaction-rewrite-base-href@5b54862a8831e012d4f1a8b2660894415fdde8ec,` (v1.1.0)
  * **Workflow permissions** > **Read repository contents and packages permissions**
* **Pages** > **Build and deployment**
  * **Source** > **GitHub Actions**
  * Selected workflow: **Static HTML** and use the [`static.yml` file](https://github.com/dotnet/blazor-samples/blob/main/.github/workflows/static.yml) for this site. Configure the following entries in the script for your deployment:
    * Publish directory (`PUBLISH_DIR`)
    * Push path (`on:push:paths`)
    * .NET SDK version (`dotnet-version` via the [`actions/setup-dotnet` Action](https://github.com/actions/setup-dotnet))
    * Publish path (`dotnet publish` command)
    * Base HREF (`base_href` for the [`SteveSandersonMS/ghaction-rewrite-base-href` Action](https://github.com/SteveSandersonMS/ghaction-rewrite-base-href))
  * **Custom domain**: Set if you intend to use a custom domain. Not used or covered for this deployment. For more information, see [Configuring a custom domain for your GitHub Pages site](https://docs.github.com/pages/configuring-a-custom-domain-for-your-github-pages-site).
  * **Enforce HTTPS**> Enabled (selected)

The GitHub-hosted Ubuntu (latest) server has a version of the .NET SDK pre-installed installed. You can remove the [`actions/setup-dotnet` Action](https://github.com/actions/setup-dotnet) step from the `static.yml` script if the pre-installed .NET SDK is sufficient to compile the app. To determine the .NET SDK installed for `ubuntu-latest`:

1. Go to the [**Available Images** section of the `actions/runner-images` GitHub repository](https://github.com/actions/runner-images?tab=readme-ov-file#available-images).
1. Locate the `ubuntu-latest` image, which is the first table row.
1. Select the link in the `Included Software` column.
1. Scroll down to the *.NET Tools* section to see the .NET Core SDK installed with the image.

## Deployment notes

The default GitHub Action, which deploys pages, skips deployment of folders starting with underscore, for example, the `_framework` folder. To deploy folders starting with underscore, add an empty `.nojekyll` file to the root of the app's repository.

Git treats JavaScript (JS) files, such as `blazor.webassembly.js`, as text and converts line endings from CRLF (carriage return-line feed) to LF (line feed) in the deployment pipeline. These changes to JS files produce different file hashes than Blazor sends to the client in the `blazor.boot.json` file. The mismatches result in integrity check failures on the client. One approach to solving this problem is to add a `.gitattributes` file with `*.js binary` line before adding the app's assets to the Git branch. The `*.js binary` line configures Git to treat JS files as binary files, which avoids processing the files in the deployment pipeline. The file hashes of the unprocessed files match the entries in the `blazor.boot.json` file, and client-side integrity checks pass. For more information, see [ASP.NET Core Blazor WebAssembly .NET bundle caching and integrity check failures](https://learn.microsoft.com/aspnet/core/blazor/host-and-deploy/webassembly/bundle-caching-and-integrity-check-failures).

To handle URL rewrites based on [Single Page Apps for GitHub Pages (`rafrex/spa-github-pages` GitHub repository)](https://github.com/rafrex/spa-github-pages):

* Add a `wwwroot/404.html` file with a script that handles redirecting the request to the `index.html` page.
* In `wwwroot/index.html`, add the script to `<head>` content.

GitHub Pages doesn't natively support using Brotli-compressed resources. To use Brotli:

* Add the `wwwroot/decode.js` script to the app.
* Add the `<script>` tag to load the `decode.js` script in the `wwwroot/index.html` file.
  * Set `autostart="false"` for the Blazor WebAssembly script.
  * Add the `loadBootResource` script after the `<script>` tag that loads the Blazor WebAssembly script.

* Add `robots.txt` and `sitemap.txt` files to improve SEO.
