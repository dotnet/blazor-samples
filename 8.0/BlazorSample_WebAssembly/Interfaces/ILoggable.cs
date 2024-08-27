using Microsoft.AspNetCore.Components;

namespace BlazorSample.Interfaces;

public interface ILoggable : IComponent
{
    public void Log();
}
