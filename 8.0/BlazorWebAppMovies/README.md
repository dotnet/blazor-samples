# Blazor Web App Movies tutorial sample app

This sample app is the completed app for the Blazor Web App Movies tutorial:

[Build a Blazor movie database app (Overview)](https://learn.microsoft.com/aspnet/core/blazor/tutorials/movie-database-app/)

## Steps to run the sample with Visual Studio on Windows OS

The sample app is configured to match the Visual Studio version of the article and requires EF Core database migrations to create the SQL Server database. If you're not using Visual Studio or you don't intend to use a SQL Server database, you can still reference the code in this sample app while reading the article.

1. Clone this repository or download a ZIP archive of the repository. For more information, see [How to download a sample](https://learn.microsoft.com/aspnet/core/introduction-to-aspnet-core#how-to-download-a-sample).

1. In Visual Studio, use [Visual Studio Connected Services](https://learn.microsoft.com/visualstudio/azure/overview-connected-services) to update the database, which creates and updates the database using the migrations in the `Migrations` folder of the sample app.
   
   a. In **Solution Explorer**, double-click **Connected Services**. In the **SQL Server Express LocalDB** area of **Service Dependencies**, select the ellipsis (`...`) followed by **Update database**.

   b. The **Update database with the latest migration** dialog opens. Wait for the **DbContext class names** field to update and for prior migrations to load, which may take a few seconds. Select the **Finish** button.
   
      When the update database command executes, a new database is created for the app with the name `BlazorWebAppMovies`. The name is taken from the app settings file (`appsettings.json`), and the database physical files are created in your Windows user folder.

   c. Select the **Close** button after the operation finishes.

1. The default URLs for the app are `https://localhost:7073` (secure HTTPs) and `http://localhost:5261` (insecure HTTP).
   
   You can use the existing URLs or update them in the `Properties/launchSettings.json` file.

1. Run the app.

## Package roll-forward behavior

The NuGet packages referenced in the project file (`.csproj`) aren't necessarily the latest patch package releases. However, patch releases in .NET roll-forward automatically when the app is built and packages are restored. There's no need to update the package versions in the project file to the latest patch releases.

## Clean up

When you're finished with the tutorial and delete the sample app from your local system, you can also delete the `BlazorWebAppMovies` database in Visual Studio's SQL Server Object Explorer (SSOX):

1. Access SSOX by selecting **View** > **SQL Server Object Explorer** from the menu bar.
1. Display the database list by selecting the triangles next to `SQL Server` > `(localdb)\MSSQLLocalDB` > `Databases`.
1. Right-click the `BlazorWebAppMovies` database in the list and select **Delete**.
1. Select the checkbox to close existing collections before selecting **OK**.

When deleting the database in SSOX, the database's physical files are removed from your Windows user folder.
