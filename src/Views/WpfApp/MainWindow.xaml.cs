using System.Windows;
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
        if (DataContext is MainViewModel viewModel) viewModel.ErrorHandler = new ErrorHandle();
    }
}