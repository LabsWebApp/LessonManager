using System.Windows;
using System.Windows.Controls;
using ViewModels;
using WpfApp.Helpers;

namespace WpfApp;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel(new ErrorHandle());
    }

    private void Grid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is DataGrid { SelectedIndex: >= 0 } grid) grid.ScrollIntoView(grid.SelectedItem);
        //if (DataContext is MainViewModel model)
        //{
        //    model.AsyncSignUpCommand.RaiseCanExecuteChanged();
        //    model.AsyncSignOutCommand.RaiseCanExecuteChanged();
        //}
    }
}