namespace BlazorSample;

public class TimerService(NotifierService notifier, 
    ILogger<TimerService> logger) : IDisposable
{
    private int elapsedCount;
    private readonly static TimeSpan heartbeatTickRate = TimeSpan.FromSeconds(5);
    private readonly ILogger<TimerService> logger = logger;
    private readonly NotifierService notifier = notifier;
    private PeriodicTimer? timer;

    public async Task Start()
    {
        if (timer is null)
        {
            timer = new(heartbeatTickRate);
            logger.LogInformation("Started");

            using (timer)
            {
                while (await timer.WaitForNextTickAsync())
                {
                    elapsedCount += 1;
                    await notifier.Update("elapsedCount", elapsedCount);
                    logger.LogInformation("elapsedCount: {elapsedCount}", 
                        elapsedCount);
                }
            }
        }
    }

    public void Dispose()
    {
        timer?.Dispose();

        // The following prevents derived types that introduce a
        // finalizer from needing to re-implement IDisposable.
        GC.SuppressFinalize(this);
    }
}
