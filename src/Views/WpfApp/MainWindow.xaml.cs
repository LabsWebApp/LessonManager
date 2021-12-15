using System;
using System.Windows.Input;
using System.Windows.Controls;
using Models.DataProviders.Helpers;
using Models.Entities.Proxies;
using ViewModels;
using ViewModels.Interfaces;
using WpfApp.Controls;
using WpfApp.Helpers;

namespace WpfApp;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow
{
    private readonly MainViewModel _model;
    public MainWindow()
    {
        InitializeComponent();
        _model = new MainViewModel(Provider.SqLite, new ErrorHandle(), new Confirmed())
        {
            AdvancedInCourses = InCoursesGrid,
            AdvancedOutCourses = OutCoursesGrid
        };

        DataContext = _model;
    }

    private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var grid = sender as DataGrid ?? throw new Exception();
        if (e.AddedItems.Count == 0) return;
        grid.ScrollIntoView(e.AddedItems[^1]!);

        if (grid.Name == "StudentsGrid") return;
        _model.AsyncSetCoursesCommand.RaiseCanExecuteChanged();
        _model.AsyncUnsetCoursesCommand.RaiseCanExecuteChanged();
    }

    private void InOutCoursesGrid_MouseUp(object sender, MouseButtonEventArgs e)
    {
        var grid = (AdvancedDataGrid)sender ?? throw new Exception();
        foreach (var item in grid.Items)
        {
            if (item is not ProxyCourse proxy) continue;
            if (grid.SelectedItems.Contains(proxy))
            {
                if (!_model.SelectedProxyCourses.Contains(proxy))
                    _model.SelectedProxyCourses.Add(proxy);
            }
            else
            {
                if (_model.SelectedProxyCourses.Contains(proxy))
                    _model.SelectedProxyCourses.Remove(proxy);
            }
        }
    }
}