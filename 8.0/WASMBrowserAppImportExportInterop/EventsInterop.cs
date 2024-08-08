using System;
using System.Runtime.InteropServices.JavaScript;
using System.Threading.Tasks;

public partial class EventsInterop
{
    [JSImport("subscribeEventById", "EventsShim")]
    public static partial JSObject SubscribeEventById(string elementId,
        string eventName,
        [JSMarshalAs<JSType.Function<JSType.String, JSType.String>>]
        Action<string, string> listenerFunc);

    [JSImport("unsubscribeEventById", "EventsShim")]
    public static partial void UnsubscribeEventById(string elementId,
        string eventName, JSObject listenerHandler);

    [JSImport("triggerClick", "EventsShim")]
    public static partial void TriggerClick(string elementId);

    [JSImport("getElementById", "EventsShim")]
    public static partial JSObject GetElementById(string elementId);

    [JSImport("subscribeEvent", "EventsShim")]
    public static partial JSObject SubscribeEvent(JSObject htmlElement,
        string eventName,
        [JSMarshalAs<JSType.Function<JSType.Object>>]
        Action<JSObject> listenerFunc);

    [JSImport("unsubscribeEvent", "EventsShim")]
    public static partial void UnsubscribeEvent(JSObject htmlElement,
        string eventName, JSObject listenerHandler);
}

public static class EventsUsage
{
    public static async Task Run()
    {
        await JSHost.ImportAsync("EventsShim", "/EventsShim.js");

        Action<string, string> listenerFunc = (eventName, elementId) =>
            Console.WriteLine(
                $"In C# event listener: Event {eventName} from ID {elementId}");

        // Assumes two buttons exist on the page with ids of "btn1" and "btn2"
        JSObject listenerHandler1 =
            EventsInterop.SubscribeEventById("btn1", "click", listenerFunc);
        JSObject listenerHandler2 =
            EventsInterop.SubscribeEventById("btn2", "click", listenerFunc);
        Console.WriteLine("Subscribed to btn1 & 2.");
        EventsInterop.TriggerClick("btn1");
        EventsInterop.TriggerClick("btn2");

        EventsInterop.UnsubscribeEventById("btn2", "click", listenerHandler2);
        Console.WriteLine("Unsubscribed btn2.");
        EventsInterop.TriggerClick("btn1");
        EventsInterop.TriggerClick("btn2"); // Doesn't trigger because unsubscribed
        EventsInterop.UnsubscribeEventById("btn1", "click", listenerHandler1);
        // Pitfall: Using a different handler for unsubscribe silently fails.
        // EventsInterop.UnsubscribeEventById("btn1", "click", listenerHandler2);

        // With JSObject as event target and event object.
        Action<JSObject> listenerFuncForElement = (eventObj) =>
        {
            string eventType = eventObj.GetPropertyAsString("type");
            JSObject target = eventObj.GetPropertyAsJSObject("target");
            Console.WriteLine(
                $"In C# event listener: Event {eventType} from " +
                $"ID {target.GetPropertyAsString("id")}");
        };

        JSObject htmlElement = EventsInterop.GetElementById("btn1");
        JSObject listenerHandler3 = EventsInterop.SubscribeEvent(
            htmlElement, "click", listenerFuncForElement);
        Console.WriteLine("Subscribed to btn1.");
        EventsInterop.TriggerClick("btn1");
        EventsInterop.UnsubscribeEvent(htmlElement, "click", listenerHandler3);
        Console.WriteLine("Unsubscribed btn1.");
        EventsInterop.TriggerClick("btn1");
    }
}
