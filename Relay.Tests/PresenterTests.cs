namespace Relay.Tests;

using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Relay.PresentationModel;

[TestClass]
public class PresenterTests
{
    private sealed class TestPresenter : Presenter
    {
        public new ICommand GetCommand(Func<Task> execute, Func<bool>? canExecute = null) =>
            base.GetCommand(execute, canExecute);

        public new ICommand GetCommand<T>(Func<T, Task> execute, Func<T, bool>? canExecute = null) =>
            base.GetCommand(execute, canExecute);
    }

    [TestMethod]
    public void GetCommand_WithoutCanExecute_ReturnsCommand()
    {
        var presenter = new TestPresenter();
        Func<Task> execute = async () => await Task.CompletedTask;

        var command = presenter.GetCommand(execute);

        Assert.IsNotNull(command);
        Assert.IsInstanceOfType(command, typeof(ICommand));
    }

    [TestMethod]
    public void GetCommand_WithCanExecute_ReturnsCommand()
    {
        var presenter = new TestPresenter();
        Func<Task> execute = async () => await Task.CompletedTask;
        Func<bool> canExecute = () => true;

        var command = presenter.GetCommand(execute, canExecute);

        Assert.IsNotNull(command);
        Assert.IsInstanceOfType(command, typeof(ICommand));
    }

    [TestMethod]
    public void GetCommand_CachesCommand()
    {
        var presenter = new TestPresenter();
        Func<Task> execute = async () => await Task.CompletedTask;
        Func<bool> canExecute = () => true;

        var command1 = presenter.GetCommand(execute, canExecute);
        var command2 = presenter.GetCommand(execute, canExecute);

        Assert.AreSame(command1, command2, "GetCommand should return the same command instance for the same execute delegate.");
    }

    [TestMethod]
    public void GetCommand_WithDifferentExecute_ReturnsDifferentCommand()
    {
        var presenter = new TestPresenter();
        Func<Task> execute1 = async () => await Task.CompletedTask;
        Func<Task> execute2 = async () => await Task.CompletedTask;

        var command1 = presenter.GetCommand(execute1);
        var command2 = presenter.GetCommand(execute2);

        Assert.AreNotSame(command1, command2, "GetCommand should return different command instances for different execute delegates.");
    }

    [TestMethod]
    public void GetCommand_WithCanExecute_ReturnsCommandThatCanExecute()
    {
        var presenter = new TestPresenter();
        Func<Task> execute = async () => await Task.CompletedTask;
        Func<bool> canExecute = () => true;

        var command = presenter.GetCommand(execute, canExecute);

        Assert.IsTrue(command.CanExecute(null), "Command should be executable when canExecute returns true.");
    }

    [TestMethod]
    public void GetCommand_WithCanExecute_ReturnsCommandThatCannotExecute()
    {
        var presenter = new TestPresenter();
        Func<Task> execute = async () => await Task.CompletedTask;
        Func<bool> canExecute = () => false;

        var command = presenter.GetCommand(execute, canExecute);

        Assert.IsFalse(command.CanExecute(null), "Command should not be executable when canExecute returns false.");
    }

    [TestMethod]
    public void GetCommandT_WithoutCanExecute_ReturnsCommand()
    {
        var presenter = new TestPresenter();
        Func<int, Task> execute = async (param) => await Task.CompletedTask;

        var command = presenter.GetCommand(execute);

        Assert.IsNotNull(command);
        Assert.IsInstanceOfType(command, typeof(ICommand));
    }

    [TestMethod]
    public void GetCommandT_WithCanExecute_ReturnsCommand()
    {
        var presenter = new TestPresenter();
        Func<int, Task> execute = async (param) => await Task.CompletedTask;
        Func<int, bool> canExecute = (param) => true;

        var command = presenter.GetCommand(execute, canExecute);

        Assert.IsNotNull(command);
        Assert.IsInstanceOfType(command, typeof(ICommand));
    }

    [TestMethod]
    public void GetCommandT_CachesCommand()
    {
        var presenter = new TestPresenter();
        Func<int, Task> execute = async (param) => await Task.CompletedTask;
        Func<int, bool> canExecute = (param) => true;

        var command1 = presenter.GetCommand(execute, canExecute);
        var command2 = presenter.GetCommand(execute, canExecute);

        Assert.AreSame(command1, command2, "GetCommand<T> should return the same command instance for the same execute delegate.");
    }

    [TestMethod]
    public void GetCommandT_WithDifferentExecute_ReturnsDifferentCommand()
    {
        var presenter = new TestPresenter();
        Func<int, Task> execute1 = async (param) => await Task.CompletedTask;
        Func<int, Task> execute2 = async (param) => await Task.CompletedTask;

        var command1 = presenter.GetCommand(execute1);
        var command2 = presenter.GetCommand(execute2);

        Assert.AreNotSame(command1, command2, "GetCommand<T> should return different command instances for different execute delegates.");
    }

    [TestMethod]
    public void GetCommandT_WithCanExecute_ReturnsCommandThatCanExecute()
    {
        var presenter = new TestPresenter();
        Func<int, Task> execute = async (param) => await Task.CompletedTask;
        Func<int, bool> canExecute = (param) => true;

        var command = presenter.GetCommand(execute, canExecute);

        Assert.IsTrue(command.CanExecute(0), "Command should be executable when canExecute returns true.");
    }

    [TestMethod]
    public void GetCommandT_WithCanExecute_ReturnsCommandThatCannotExecute()
    {
        var presenter = new TestPresenter();
        Func<int, Task> execute = async (param) => await Task.CompletedTask;
        Func<int, bool> canExecute = (param) => false;

        var command = presenter.GetCommand(execute, canExecute);

        Assert.IsFalse(command.CanExecute(0), "Command should not be executable when canExecute returns false.");
    }
}
