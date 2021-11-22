using Models;
using Models.Entities;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using Models.DataProviders;
using ViewModelBase.Commands;
using ViewModelBase.Commands.AsyncCommands;
using ViewModelBase.Commands.ErrorHandlers;
using ViewModelBase.Commands.QuickCommands;

namespace ViewModels;

public class MainViewModel : ViewModelBase.ViewModelBase
{
    #region init
    private readonly DataManager data;
    private const string PatternName = @"^[А-ЯЁ][а-яё]";
    private bool _isBusy;

    public ObservableCollection<Student> Students { get; set; }
    public ObservableCollection<Course> Courses { get; set; }
    public ObservableCollection<Course>? OutCourses { get; set; }
    public ObservableCollection<Course>? InCourses { get; set; }

    public MainViewModel(Provider provider, IErrorHandler errorHandler)
    {
        data = DataManager.Get(provider);
        Students = new ObservableCollection<Student>(data.StudentsRep.Items);
        Courses = new ObservableCollection<Course>(data.CoursesRep.Items);

        AsyncStudentCreateCommand = new AsyncCommand(CreateStudentAsync, () => CanCreate(_newStudent), errorHandler);
        AsyncCourseCreateCommand = new AsyncCommand(CreateCourseAsync, () => CanCreate(_newCourse), errorHandler);
        StudentFindCommand = new Command(StudentFind,
            () => Students.Any() && _findStudent.Trim().Length > 0, errorHandler);
        CourseFindCommand = new Command(CourseFind,
            () => Courses.Any() && _findCourse.Trim().Length > 0, errorHandler);
    }
    #endregion

    #region create
    private bool CanCreate(string name)
    {
        if (_isBusy) return false;
        name = name.Trim();
        return !string.IsNullOrEmpty(name) && Regex.IsMatch(name, PatternName);
    }

    private string _newStudent = string.Empty;
    public string NewStudent
    {
        set
        {
            if (set(ref _newStudent, value))
                AsyncStudentCreateCommand.RaiseCanExecuteChanged();
        }
    }

    private string _newCourse = string.Empty;
    public string NewCourse
    {
        set
        {
            if (set(ref _newCourse, value))
                AsyncCourseCreateCommand.RaiseCanExecuteChanged();
        }
    }
    public AsyncCommand AsyncStudentCreateCommand { get; }
    private async Task CreateStudentAsync()
    {
        _isBusy = true;
        try
        {
            Student student = new() { Name = _newStudent };
            await data.StudentsRep.AddAsync(student);
            Students.Insert(0, student);
            SelectedStudent = student;
            NewStudent = string.Empty;
        }
        finally
        {
            _isBusy = false;
        }
    }

    public AsyncCommand AsyncCourseCreateCommand { get; }
    private async Task CreateCourseAsync()
    {
        _isBusy = true;
        try
        {
            Course course = new() { Name = _newCourse };
            await data.CoursesRep.AddAsync(course);
            Courses.Insert(0, course);
            SelectedCourse = course;
            NewCourse = string.Empty;
        }
        finally
        {
            _isBusy = false;
        }
    }
    #endregion

    #region find
    private string _findStudent = string.Empty;
    public string FindStudent
    {
        set
        {
            if (set(ref _findStudent, value))
                StudentFindCommand.RaiseCanExecuteChanged();
        }  
    }

    private string _findCourse = string.Empty;
    public string FindCourse
    {
        set
        {
            if (set(ref _findCourse, value))
                CourseFindCommand.RaiseCanExecuteChanged();
        }
    }

    public Command StudentFindCommand { get; }
    public Command CourseFindCommand { get; }

    private void StudentFind()
    {
        var index = _selectedStudent is null ? 0 : Students.IndexOf(_selectedStudent);
        var result = Students
            .Skip(index + 1)
            .Union(Students.Take(index + 1))
            .FirstOrDefault(s => s.Name.ToLower().Contains(_findStudent.ToLower()));
        SelectedStudent = result ?? throw new ResultNotFoundException(_findStudent);
    }
    private void CourseFind()
    {
        var index = _selectedCourse is null ? 0 : Courses.IndexOf(_selectedCourse);
        var result = Courses
            .Skip(index + 1)
            .Union(Courses.Take(index + 1))
            .FirstOrDefault(s => s.Name.ToLower().Contains(_findCourse.ToLower()));
        SelectedCourse = result ?? throw new ResultNotFoundException(_findCourse);
    }
    #endregion

    #region selected
    private Student? _selectedStudent;
    public Student? SelectedStudent
    {
        get => _selectedStudent;
        set
        {
            if (set(ref _selectedStudent, value))
                SetInOutCourses();
        }
    }

    private void SetInOutCourses()
    {
        InCourses = SelectedStudent is null
            ? new ObservableCollection<Course>()
            : new ObservableCollection<Course>(
                data.CoursesRep.Items.Where(c => c.Students.Contains(SelectedStudent)));
        OnPropertyChanged(nameof(InCourses));

        OutCourses = SelectedStudent is null
            ? new ObservableCollection<Course>(Courses)
            : new ObservableCollection<Course>(
                data.CoursesRep.Items.Where(c => !c.Students.Contains(SelectedStudent)));
        OnPropertyChanged(nameof(OutCourses));
    }

    private Course? _selectedCourse;
    public Course? SelectedCourse
    {
        get => _selectedCourse;
        set
        {
            if (set(ref _selectedCourse, value)) ;
        }
    }
    #endregion
}