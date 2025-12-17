var builder = DistributedApplication.CreateBuilder(args);

var weatherApi = builder.AddProject<Projects.MinimalApiJwt>("weatherapi");

builder.AddProject<Projects.BlazorWebAppEntra>("blazorfrontend")
    .WithReference(weatherApi);

builder.Build().Run();
