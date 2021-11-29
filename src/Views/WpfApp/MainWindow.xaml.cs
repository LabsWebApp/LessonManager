using System;
using System.Windows.Controls;
using Models.DataProviders;
using ViewModels;
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
            StudentsRefreshable = StudentsGrid,
            CoursesRefreshable = CoursesGrid,
            AdvancedInCourses = InCoursesGrid,
            AdvancedOutCourses = OutCoursesGrid
        };

        DataContext = _model;
    }

    private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var grid = sender as DataGrid ?? throw new Exception();
        if (grid.SelectedIndex >= 0) grid.ScrollIntoView(grid.SelectedItem);

        if (grid.Name == "StudentsGrid") return;
        _model.AsyncSetCoursesCommand.RaiseCanExecuteChanged();
        _model.AsyncUnsetCoursesCommand.RaiseCanExecuteChanged();

        //if (grid.Name != "CoursesGrid") return;
        //if (InCoursesGrid.SelectedItems.Count == 1 &&
        //    !InCoursesGrid.SelectedItems.Contains(CoursesGrid.SelectedItem))
        //{
        //    InCoursesGrid.SelectedIndex = -1;
        //    OutCoursesGrid.SelectedItems.Add(CoursesGrid.SelectedItem);
        //}

        //if (OutCoursesGrid.SelectedItems.Count != 1 ||
        //    OutCoursesGrid.SelectedItems.Contains(CoursesGrid.SelectedItem)) return;
        //OutCoursesGrid.SelectedIndex = -1;
        //InCoursesGrid.SelectedItems.Add(CoursesGrid.SelectedItem);
    }
}