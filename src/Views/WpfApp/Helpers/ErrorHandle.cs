using System;
using System.Windows;
using ViewModelBase.Commands.ErrorHandlers;

namespace WpfApp.Helpers;

public class ErrorHandle : IErrorCancelHandler, IErrorNotFoundHandler
{
    public void HandleError(Exception ex) =>
        MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);

    public void HandleCancel(OperationCanceledException ex) =>
        MessageBox.Show(ex.Message, "Отмена", MessageBoxButton.OK, MessageBoxImage.Stop);

    public void HandleResultNotFound(ResultNotFoundException ex) =>
        MessageBox.Show("Вы искали: \"" + ex.Message + "\"",
            "Совпадений не найдено", MessageBoxButton.OK, MessageBoxImage.Information);
}