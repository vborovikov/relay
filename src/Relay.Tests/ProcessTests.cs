namespace Relay.Tests;

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProcessModel;

[TestClass]
public class ProcessTests
{
    private class SimpleContext
    {
        public string Text { get; set; }

        public int Number { get; set; }
    }

    [TestMethod]
    public async Task Run_SingleActivity_Executed()
    {
        var context = new[] { "A" };
        var process = Process
            .CreateProcess(context)
            .AddActivity((ctx, ct) => { ctx[0] = "B"; }, (ctx, ct) => { ctx[0] = "C"; })
            .BuildProcess();

        using (process)
        {
            await process.RunAsync(CancellationToken.None);
        }

        Assert.AreEqual("B", context[0]);
    }

    [TestMethod]
    public async Task LongRunningProcess_Canceled_Stopped()
    {
        var process = Process
            .CreateProcess(new { Timeout = TimeSpan.FromMinutes(1) })
            .AddActivity(
                (ctx, ct) => Task.Delay(ctx.Timeout, ct),
                (ctx, ct) => Task.Delay(ctx.Timeout, ct))
            .BuildProcess();

        await process.StartAsync();
        await Task.Delay(200);
        var state = await process.StopAsync();

        Assert.AreEqual(ProcessStatus.Executing, state.ProcessStatus);
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public async Task LongRunningProcess_StartedTwice_Invalid()
    {
        var process = Process
            .CreateProcess(new { Timeout = TimeSpan.FromMinutes(1) })
            .AddActivity(
                async (ctx, ct) => { await Task.Delay(ctx.Timeout, ct); },
                async (ctx, ct) => { await Task.Delay(ctx.Timeout, ct); })
            .BuildProcess();

        await process.StartAsync();
        await process.StartAsync();
    }

    [TestMethod]
    public async Task LongRunningProcess_Aborted_AbortedState()
    {
        var process = Process
            .CreateProcess(new { Timeout = TimeSpan.FromMinutes(1) })
            .AddActivity(
                (ctx, ct) => Task.Delay(ctx.Timeout, ct),
                (ctx, ct) => Task.Delay(ctx.Timeout, ct))
            .BuildProcess();

        await process.StartAsync();
        await Task.Delay(200);
        var state = await process.AbortAsync();

        Assert.AreEqual(ProcessStatus.Aborted, state.ProcessStatus);
    }

    [TestMethod]
    public async Task SharedState_DoubleProcess_ProperlyExecuted()
    {
        const int activityCount = 1000;
        var context = new SimpleContext { Text = "Test 0", Number = 0 };

        var seedProcess = BuildProcess(Process.CreateProcess(context), activityCount);
        await seedProcess.StartAsync();
        var seedState = await seedProcess.StopAsync();

        var process1 = BuildProcess(Process.LoadProcess(seedState, context), activityCount);
        var process2 = BuildProcess(Process.LoadProcess(seedState, context), activityCount);

        await process1.StartAsync();
        await process2.StartAsync();

        await Task.WhenAll(process1.ProcessingTask, process2.ProcessingTask);

        Assert.AreEqual(activityCount, context.Number);
    }

    private static Process BuildProcess(Process.Builder.ActivityBuilder<SimpleContext> seedProcessBuilder, int activityCount)
    {
        for (var i = 1; i <= activityCount; ++i)
        {
            seedProcessBuilder = seedProcessBuilder.AddActivity(
                (ctx, ct) => { ctx.Text = $"Test {++ctx.Number}"; },
                (ctx, ct) => { ctx.Text = $"Test {--ctx.Number}"; });
        }

        return seedProcessBuilder.BuildProcess();
    }
}
