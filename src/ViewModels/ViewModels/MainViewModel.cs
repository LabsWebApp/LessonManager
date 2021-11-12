using Models;
using Models.Entities;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using Models.DataProviders;
using ViewModelBase.Commands;

namespace ViewModels;

public class MainViewModel : ViewModelBase.ViewModelBase
{
    private bool _isBusy;
    public IErrorHandler? ErrorHandler { private get; set; }
    private string? _newStudent;
    public string? NewStudent
    {
        private get => _newStudent;
        set => set(ref _newStudent, value);
    }

    private const string  PatternName = @"^[А-ЯЁ][а-яё]";

    //public AsyncCommand AsyncStudentCreateCommand { get; }
    public ObservableCollection<Student>? Students { get; set; }
    public ObservableCollection<Course>? Courses { get; set; }
    private readonly DataManager data;
    public MainViewModel()
    {
        data = DataManager.Get(Provider.SqLite);
        Students = new ObservableCollection<Student>(data.StudentsRep.Items);
        Courses = new ObservableCollection<Course>(data.CoursesRep.Items);

        //AsyncStudentCreateCommand = new AsyncCommand(CreateStudentAsync, CanExecuteCreateStudent, ErrorHandler);

    }

    private bool CanExecuteCreateStudent(string? student)
    {
        if (_isBusy) return false;
        var res = student?.Trim();
        if (string.IsNullOrWhiteSpace(res)) return false;
        return Regex.IsMatch(res, PatternName);
    }

    private bool CanExecuteCreateStudent() => CanExecuteCreateStudent(NewStudent);
    #region Свойства
    #endregion
}