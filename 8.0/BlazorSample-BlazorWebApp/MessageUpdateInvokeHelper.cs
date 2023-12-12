using Microsoft.JSInterop;

namespace BlazorSample;

public class MessageUpdateInvokeHelper(Action action)
{
    private readonly Action action = action;

    [JSInvokable]
    public void UpdateMessageCaller()
    {
        action.Invoke();
    }
}
