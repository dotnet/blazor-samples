var builder = DistributedApplication.CreateBuilder(args);

var weatherApi = builder.AddProject<Projects.MinimalApiJwt>("weatherapi");

builder.AddProject<Projects.BlazorWebAppOidc>("blazorfrontend")
    .WithReference(weatherApi);

builder.Build().Run();
