using System;
using System.Windows;
using ViewModelBase.Commands;
using ViewModelBase.Commands.ErrorHandlers;

namespace WpfApp.Helpers;

public class ErrorHandle : IErrorCancelHandler, IErrorNotFoundHandler, IWarningHandler
{
    public void HandleError(Exception ex)
    {
        if (ex.Message != IErrorHandler.NoHandle)
            MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
    }

    public void HandleCancel(OperationCanceledException ex)
    {
        if (ex.Message != IErrorHandler.NoHandle)
            MessageBox.Show(
                string.IsNullOrEmpty(ex.Message) || ex.Message == "A task was canceled." ?
                    "Операция успешно отменена." : ex.Message,
                "Отмена", MessageBoxButton.OK, MessageBoxImage.Asterisk);
    }

    public void HandleResultNotFound(ResultNotFoundException ex)
    {
        if (ex.Message != IErrorHandler.NoHandle)
            MessageBox.Show("Вы искали: \"" + ex.Message + "\"",
                "Совпадений не найдено", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    public void WarningHandle(WarningException ex)
    {
        if (ex.Message != IErrorHandler.NoHandle)
            MessageBox.Show(ex.Message, "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
    }
}