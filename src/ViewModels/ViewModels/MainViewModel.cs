using Models;
using Models.Entities;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using Models.DataProviders;
using ViewModelBase.Commands;
using ViewModelBase.Commands.AsyncCommands;
using ViewModelBase.Commands.QuickCommands;

namespace ViewModels;

public class MainViewModel : ViewModelBase.ViewModelBase
{
    private const string PatternName = @"^[А-ЯЁ][а-яё]";
    private bool _isBusy;
   // public IErrorHandler? ErrorHandler { private get; set; }
    private string _newStudent = string.Empty;
    public string NewStudent
    {
        private get => _newStudent;
        set
        {
            if (set(ref _newStudent, value))
                AsyncStudentCreateCommand.RaiseCanExecuteChanged();
        }
    }

    private string _findStudent = string.Empty;
    public string FindStudent
    {
        private get => _findStudent;
        set
        {
            if (set(ref _findStudent, value))
                StudentFindCommand.RaiseCanExecuteChanged();
        }
    }

    private Student? _selectedStudent;
    public Student? SelectedStudent
    {
        private get => _selectedStudent;
        set
        {
            if (set(ref _selectedStudent, value));
        }
    }

    public AsyncCommand AsyncStudentCreateCommand { get; }
    public Command StudentFindCommand { get; }
    public ObservableCollection<Student> Students { get; set; }
    public ObservableCollection<Course> Courses { get; set; }
    private readonly DataManager data;
    public MainViewModel(IErrorHandler? errorHandler = null)
    {
        data = DataManager.Get(Provider.SqlServer);
        Students = new ObservableCollection<Student>(data.StudentsRep.Items);
        Courses = new ObservableCollection<Course>(data.CoursesRep.Items);

        AsyncStudentCreateCommand = new AsyncCommand(CreateStudentAsync, CanCreateStudent, errorHandler);
        StudentFindCommand = new Command(StudentFind, 
            () => !_isBusy && FindStudent.Trim().Length > 0, errorHandler);
    }

    private void StudentFind()
    {
        var index = SelectedStudent is null ? 0 : Students.IndexOf(SelectedStudent);
        var result = Students
            .Skip(index + 1)
            .Concat(Students.Take(index))
            .FirstOrDefault(s => s.Name.ToLower().Contains(FindStudent.ToLower()));
        if (result is null)
            throw new Exception("Студент не найден");
        SelectedStudent = result;
    }

    private bool CanCreateStudent()
    {
        if (_isBusy) return false;
        return Regex.IsMatch(_newStudent, PatternName);
    }

    private async Task CreateStudentAsync()
    {
        _isBusy = true;
        try
        {
            if (Students.FirstOrDefault(s => string.Equals(s.Name, _newStudent, StringComparison.CurrentCultureIgnoreCase))
                != default)
                throw new Exception("Такой студент уже есть");
            Student student = new() { Name = NewStudent };
            await data.StudentsRep.AddAsync(student);
            Students.Insert(0,student);
            NewStudent = string.Empty;
        }
        finally
        {
            _isBusy = false;
        }
    }

    private bool CanExecuteCreateStudent(string? student)
    {
        if (_isBusy) return false;
        var res = student?.Trim();
        if (string.IsNullOrWhiteSpace(res)) return false;
        return Regex.IsMatch(res, PatternName);
    }

    #region Свойства
    #endregion
}