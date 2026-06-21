using System;
using System.Windows.Input;

namespace NebulaControls.Controls;

internal sealed class NebulaRelayCommand : ICommand
{
    private readonly Action<object?> execute;
    private readonly Predicate<object?>? canExecute;

    public NebulaRelayCommand(Action<object?> execute, Predicate<object?>? canExecute = null)
    {
        this.execute = execute;
        this.canExecute = canExecute;
    }

    public event EventHandler? CanExecuteChanged;

    public bool CanExecute(object? parameter)
    {
        return canExecute?.Invoke(parameter) ?? true;
    }

    public void Execute(object? parameter)
    {
        execute(parameter);
    }

    public void RaiseCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
