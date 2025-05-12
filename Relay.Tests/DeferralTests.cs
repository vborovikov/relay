namespace Relay.Tests;

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Relay.InteractionModel;

[TestClass]
public class DeferralTests
{
    [TestMethod]
    public void GetDeferral_ShouldReturnDeferral()
    {
        // Arrange
        var args = new DeferredEventArgs();

        // Act
        var deferral = args.GetDeferral();

        // Assert
        Assert.IsNotNull(deferral);
    }

    [TestMethod]
    public void GetCurrentDeferralAndReset_ShouldReturnDeferralAndReset()
    {
        // Arrange
        var args = new DeferredEventArgs();
        var deferral = args.GetDeferral();

        // Act
        var currentDeferral = args.GetCurrentDeferralAndReset();

        // Assert
        Assert.IsNotNull(currentDeferral);
        Assert.IsNull(args.GetCurrentDeferralAndReset());
    }

    [TestMethod]
    public void Complete_ShouldSetResult()
    {
        // Arrange
        var deferral = new EventDeferral();

        // Act
        deferral.Complete();

        // Assert
        // no exception should be thrown when waiting for completion
        var task = deferral.WaitForCompletion(CancellationToken.None);
        Assert.IsTrue(task.IsCompleted);
    }

    [TestMethod]
    public async Task WaitForCompletion_ShouldCompleteWhenDeferralIsCompleted()
    {
        // Arrange
        var deferral = new EventDeferral();
        var cts = new CancellationTokenSource();

        // Act
        var waitTask = deferral.WaitForCompletion(cts.Token);
        deferral.Complete();

        // Assert
        await waitTask; // should complete without exception
    }

    [TestMethod]
    public async Task WaitForCompletion_ShouldBeCanceled()
    {
        // Arrange
        var deferral = new EventDeferral();
        var cts = new CancellationTokenSource();
        cts.Cancel(); // cancel immediately

        // Act & Assert
        await Assert.ThrowsExceptionAsync<TaskCanceledException>(() => deferral.WaitForCompletion(cts.Token));
    }

    private class TestEventArgs : DeferredEventArgs { }

    [TestMethod]
    public async Task InvokeAsync_ShouldInvokeHandlers()
    {
        // Arrange
        var eventHandler = new EventHandler<TestEventArgs>((sender, e) => { /* handler logic */ });
        var args = new TestEventArgs();
        var sender = new object();

        // Act
        await eventHandler.InvokeAsync(sender, args);

        // Assert
        // no exceptions should be thrown
    }

    [TestMethod]
    public async Task InvokeAsync_ShouldHandleCancellation()
    {
        // Arrange
        var eventHandler = new EventHandler<TestEventArgs>((sender, e) => { /* handler logic */ });
        var args = new TestEventArgs();
        var sender = new object();
        var cts = new CancellationTokenSource();
        cts.Cancel(); // cancel immediately

        // Act & Assert
        await Assert.ThrowsExceptionAsync<OperationCanceledException>(() => eventHandler.InvokeAsync(sender, args, cts.Token));
    }

    [TestMethod]
    public void InvokeAsync_ShouldReturnCompletedTask_WhenNoHandlers()
    {
        // Arrange
        EventHandler<TestEventArgs>? eventHandler = null; // no handlers
        var args = new TestEventArgs();
        var sender = new object();

        // Act
        var task = eventHandler.InvokeAsync(sender, args);

        // Assert
        Assert.IsTrue(task.IsCompleted);
    }
}
