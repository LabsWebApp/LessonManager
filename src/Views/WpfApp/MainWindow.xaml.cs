using System.Windows.Controls;
using System.Windows.Data;
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
        DataContext = new MainViewModel(
            Provider.SqlServer, new ErrorHandle(), new Confirmed());
        var data = (DataContext as MainViewModel)!;
        BindingOperations.EnableCollectionSynchronization(
            data.ProxyStudents, data.ItemLock);
        BindingOperations.EnableCollectionSynchronization(
            data.ProxyCourses, data.ItemLock);
        BindingOperations.EnableCollectionSynchronization(
            data.InProxyCourses, data.ItemLock);
        BindingOperations.EnableCollectionSynchronization(
            data.OutProxyCourses, data.ItemLock);
    }

    private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems.Count > 0 && sender is DataGrid grid)
            grid.ScrollIntoView(e.AddedItems[^1]!);
    }
}