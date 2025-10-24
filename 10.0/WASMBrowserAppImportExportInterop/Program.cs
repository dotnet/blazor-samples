using System.Threading.Tasks;

namespace WASMBrowserApp;

internal class Program
{
    private static async Task Main(string[] args)
    {
        await PrimitivesUsage.Run();
        await DateUsage.Run();
        await JSObjectUsage.Run();
        await PromisesUsage.Run();
        await EventsUsage.Run();
        JSObjectBenchmark.Run();
    }
}
