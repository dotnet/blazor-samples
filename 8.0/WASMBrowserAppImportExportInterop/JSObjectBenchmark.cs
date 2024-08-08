using System;
using System.Diagnostics;

public static class JSObjectBenchmark
{
    public static void Run()
    {
        Stopwatch sw = new();
        var jsObject = JSObjectInterop.CreateObject();

        sw.Start();

        for (int i = 0; i < 1000000; i++)
        {
            JSObjectInterop.IncrementAnswer(jsObject);
        }

        sw.Stop();

        Console.WriteLine(
            $"JS interop elapsed time: {sw.Elapsed.TotalSeconds:#.0000} seconds " +
            $"at {sw.Elapsed.TotalMilliseconds / 1000000d:#.000000} ms per " +
            "operation");

        var pocoObject =
            new PocoObject { Question = "What is the answer?", Answer = 41 };
        sw.Restart();

        for (int i = 0; i < 1000000; i++)
        {
            pocoObject.IncrementAnswer();
        }

        sw.Stop();

        Console.WriteLine($".NET elapsed time: {sw.Elapsed.TotalSeconds:#.0000} " +
            $"seconds at {sw.Elapsed.TotalMilliseconds / 1000000d:#.000000} ms " +
            "per operation");

        Console.WriteLine($"Begin Object Creation");

        sw.Restart();

        for (int i = 0; i < 1000000; i++)
        {
            var jsObject2 = JSObjectInterop.CreateObject();
            JSObjectInterop.IncrementAnswer(jsObject2);
        }

        sw.Stop();

        Console.WriteLine(
            $"JS interop elapsed time: {sw.Elapsed.TotalSeconds:#.0000} seconds " +
            $"at {sw.Elapsed.TotalMilliseconds / 1000000d:#.000000} ms per " +
            "operation");

        sw.Restart();

        for (int i = 0; i < 1000000; i++)
        {
            var pocoObject2 =
                new PocoObject { Question = "What is the answer?", Answer = 0 };
            pocoObject2.IncrementAnswer();
        }

        sw.Stop();
        Console.WriteLine(
            $".NET elapsed time: {sw.Elapsed.TotalSeconds:#.0000} seconds at " +
            $"{sw.Elapsed.TotalMilliseconds / 1000000d:#.000000} ms per operation");
    }

    public class PocoObject // Plain old CLR object
    {
        public string Question { get; set; }
        public int Answer { get; set; }

        public void IncrementAnswer() => Answer += 1;
    }
}
