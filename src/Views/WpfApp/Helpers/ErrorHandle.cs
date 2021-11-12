using System;
using System.Windows;
using ViewModelBase.Commands;

namespace WpfApp.Helpers;

public class ErrorHandle : IErrorCancelHandler
{
    public void HandleError(Exception ex) =>
        MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);

    public void HandleCancel(OperationCanceledException ex) =>
        MessageBox.Show(ex.Message, "Отмена", MessageBoxButton.OK, MessageBoxImage.Stop);
}