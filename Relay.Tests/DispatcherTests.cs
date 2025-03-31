namespace Relay.Tests;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Relay.RequestModel;
using Relay.RequestModel.Default;
using System;
using System.Threading;
using System.Threading.Tasks;

[TestClass]
public class DefaultRequestDispatcherTests
{
    #region Test Classes

    public record TestCommand : ICommand
    {
        public CancellationToken CancellationToken { get; init; }
    }
    public class TestQuery : IQuery<int>
    {
        public CancellationToken CancellationToken { get; init; }
    }

    public class TestRequestHandler :
        IAsyncCommandHandler<TestCommand>,
        ICommandHandler<TestCommand>,
        IAsyncQueryHandler<TestQuery, int>,
        IQueryHandler<TestQuery, int>
    {
        public Task ExecuteAsync(TestCommand command)
        {
            return Task.CompletedTask;
        }

        public void Execute(TestCommand command)
        {
        }

        public Task<int> RunAsync(TestQuery query)
        {
            return Task.FromResult(1);
        }

        public int Run(TestQuery query)
        {
            return 1;
        }
    }

    #endregion

    [TestMethod]
    public async Task ExecuteAsync_AsyncCommandHandler_Success()
    {
        // Arrange
        var handler = new TestRequestHandler();
        IRequestDispatcher dispatcher = DefaultRequestDispatcher.From(handler);
        var command = new TestCommand();

        // Act
        await dispatcher.ExecuteAsync(command);

        // Assert
        // No exception means success
    }

    [TestMethod]
    public async Task ExecuteAsync_SyncCommandHandler_Success()
    {
        // Arrange
        var handler = new TestRequestHandler();
        IRequestDispatcher dispatcher = DefaultRequestDispatcher.From(handler);
        var command = new TestCommand();

        // Act
        await dispatcher.ExecuteAsync(command);

        // Assert
        // No exception means success
    }

    [TestMethod]
    [ExpectedException(typeof(NotImplementedException))]
    public async Task ExecuteAsync_NoHandler_ThrowsNotImplementedException()
    {
        // Arrange
        var handler = new object();
        IRequestDispatcher dispatcher = DefaultRequestDispatcher.From(handler);
        var command = new TestCommand();

        // Act
        await dispatcher.ExecuteAsync(command);

        // Assert
        // Exception is thrown
    }

    [TestMethod]
    public async Task RunAsync_AsyncQueryHandler_Success()
    {
        // Arrange
        var handler = new TestRequestHandler();
        IRequestDispatcher dispatcher = DefaultRequestDispatcher.From(handler);
        var query = new TestQuery();

        // Act
        var result = await dispatcher.RunAsync(query);

        // Assert
        Assert.AreEqual(1, result);
    }

    [TestMethod]
    public async Task RunAsync_SyncQueryHandler_Success()
    {
        // Arrange
        var handler = new TestRequestHandler();
        IRequestDispatcher dispatcher = DefaultRequestDispatcher.From(handler);
        var query = new TestQuery();

        // Act
        var result = await dispatcher.RunAsync(query);

        // Assert
        Assert.AreEqual(1, result);
    }

    [TestMethod]
    [ExpectedException(typeof(NotImplementedException))]
    public async Task RunAsync_NoHandler_ThrowsNotImplementedException()
    {
        // Arrange
        var handler = new object();
        IRequestDispatcher dispatcher = DefaultRequestDispatcher.From(handler);
        var query = new TestQuery();

        // Act
        await dispatcher.RunAsync(query);

        // Assert
        // Exception is thrown
    }
}
