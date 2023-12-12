using Microsoft.AspNetCore.Components.Server.Circuits;

namespace BlazorSample;

public class TrackingCircuitHandler : CircuitHandler
{
    private readonly HashSet<Circuit> circuits = [];

    public override Task OnConnectionUpAsync(Circuit circuit, 
        CancellationToken cancellationToken)
    {
        circuits.Add(circuit);

        return Task.CompletedTask;
    }

    public override Task OnConnectionDownAsync(Circuit circuit, 
        CancellationToken cancellationToken)
    {
        circuits.Remove(circuit);

        return Task.CompletedTask;
    }

    public int ConnectedCircuits => circuits.Count;
}
