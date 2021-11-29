using System.Windows;
using ViewModels;
using ViewModels.Interfaces;

namespace WpfApp.Helpers;

public class Confirmed : IConfirmed
{
    public bool Confirm(string message, string caption = "") =>
        MessageBox.Show(message, caption == "" ? "Удалить безвозвратно" : caption,
            MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes;
}