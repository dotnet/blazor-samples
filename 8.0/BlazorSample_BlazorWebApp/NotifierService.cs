namespace BlazorSample;

public class NotifierService
{
    public async Task Update(string key, int value)
    {
        if (Notify != null)
        {
            await Notify.Invoke(key, value);
        }
    }

	public async Task DispatchException(Exception ex)
	{
		if (NotifyException != null)
		{
			await NotifyException.Invoke(ex);
		}
	}

	public event Func<string, int, Task>? Notify;
	public event Func<Exception, Task>? NotifyException;
}
