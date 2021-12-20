using System;
using System.Windows.Controls;
using Models.DataProviders.Helpers;
using ViewModels;
using WpfApp.Helpers;

namespace WpfApp;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel(Provider.SqLite, new ErrorHandle(), new Confirmed());
    }

    private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems.Count > 0 && sender is DataGrid grid)
            grid.ScrollIntoView(e.AddedItems[^1]!);
    }
}