using System.Windows.Input;

namespace ViewModelBase.Commands.AsyncCommands;

public interface IAsyncCommand : ICommand
{
    Task ExecuteAsync();
    bool CanExecute();
}