using System.Windows.Input;

namespace ViewModelBase.Commands.AsyncCommands;

public interface IAsyncCommand<in T> : ICommand
{
    Task ExecuteAsync(T parameter);
    bool CanExecute(T? parameter);
}