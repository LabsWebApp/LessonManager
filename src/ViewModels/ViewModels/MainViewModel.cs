using System.Collections;
using Models;
using Models.Entities;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using Models.DataProviders;
using ViewModelBase.Commands;
using ViewModelBase.Commands.AsyncCommands;
using ViewModelBase.Commands.ErrorHandlers;
using ViewModelBase.Commands.QuickCommands;
using ViewModels.Interfaces;

namespace ViewModels;

public class MainViewModel : ViewModelBase.ViewModelBase
{
    #region init
    private readonly DataManager _data;
    private const string PatternName = @"^[А-ЯЁ][а-яё]";
    private bool _isBusy;
    private readonly IConfirmed _confirmed;

    public IRefreshable? StudentsRefreshable { private get; set; } 
    public IRefreshable? CoursesRefreshable { private get; set; } 
    public IAdvancedSelectedItems? AdvancedInCourses { private get; set; } 
    public IAdvancedSelectedItems? AdvancedOutCourses { private get; set; } 
    public ObservableCollection<Student> Students { get; set; }
    public ObservableCollection<Course> Courses { get; set; }
    public ObservableCollection<Course>? OutCourses { get; set; }
    public ObservableCollection<Course>? InCourses { get; set; }

    public MainViewModel(Provider provider, IErrorHandler errorHandler, IConfirmed confirmed)
    {
        _confirmed = confirmed;
        _data = DataManager.Get(provider);
        Students = new ObservableCollection<Student>(_data.StudentsRep.Items);
        Courses = new ObservableCollection<Course>(_data.CoursesRep.Items);

        AsyncStudentCreateCommand = new AsyncCommand(CreateStudentAsync, () => CanCreate(_newStudent), errorHandler);
        AsyncCourseCreateCommand = new AsyncCommand(CreateCourseAsync, () => CanCreate(_newCourse), errorHandler);
        StudentFindCommand = new Command(StudentFind,
            () => Students.Any() && _findStudent.Trim().Length > 0, errorHandler);
        CourseFindCommand = new Command(CourseFind,
            () => Courses.Any() && _findCourse.Trim().Length > 0, errorHandler);
        AsyncStudentDeleteCommand = new AsyncCommand(StudentDeleteAsync, 
            () => !_isBusy && SelectedStudent is not null,errorHandler);
        AsyncCourseDeleteCommand = new AsyncCommand(CourseDeleteAsync,
            () => !_isBusy && SelectedCourse is not null, errorHandler);
        AsyncSetCoursesCommand = new AsyncCommand<IEnumerable?>(
            SetCoursesAsync,
            CanSetUnset, errorHandler);
        AsyncUnsetCoursesCommand = new AsyncCommand<IEnumerable?>(
            UnsetCourseAsync,
            CanSetUnset,
            errorHandler);
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
            {
                AsyncCourseCreateCommand.RaiseCanExecuteChanged();
                SetInOutCourses();
            }
        }
    }
    public AsyncCommand AsyncStudentCreateCommand { get; }
    private async Task CreateStudentAsync()
    {
        _isBusy = true;
        try
        {
            Student student = new() { Name = _newStudent };
            await _data.StudentsRep.AddAsync(student);
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
            await _data.CoursesRep.AddAsync(course);
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

    #region delete
    public AsyncCommand AsyncStudentDeleteCommand { get; }
    public AsyncCommand AsyncCourseDeleteCommand { get; }

    private async Task StudentDeleteAsync()
    {
        _isBusy = true;
        try
        {
            if (_confirmed.Confirm($"Вы действительно хотите отчислит студента: {SelectedStudent?.Name}?"))
                throw new OperationCanceledException("Отчисление отменено.");
            if (SelectedStudent is not null)
                await _data.StudentsRep.DeleteAsync(SelectedStudent.Id);
            Students.Remove(SelectedStudent!);
            SelectedStudent = null;
            CoursesRefreshable?.Refresh();
        }
        finally
        {
            _isBusy = false;
            SetInOutCourses();
        }
    }
    private async Task CourseDeleteAsync()
    {
        _isBusy = true;
        try
        {
            if (_confirmed.Confirm($"Вы действительно хотите удалить курс: \"{SelectedCourse?.Name}\"?"))
                throw new OperationCanceledException("Удаление отменено.");
            if (SelectedCourse is not null)
                await _data.CoursesRep.DeleteAsync(SelectedCourse.Id);
            Courses.Remove(SelectedCourse!);
            SelectedCourse = null;
            StudentsRefreshable?.Refresh();
        }
        finally
        {
            _isBusy = false;
            SetInOutCourses();
        }
    }
    #endregion

    #region set
    public AsyncCommand<IEnumerable?> AsyncSetCoursesCommand { get; }
    public AsyncCommand<IEnumerable?> AsyncUnsetCoursesCommand { get; }

    private bool CanSetUnset(IEnumerable? obj) =>
        !_isBusy && (obj?.Cast<Course>().Any() ?? false);

    private async Task SetCoursesAsync(IEnumerable? courses)
    {
        _isBusy = true;
        try
        {
            _isBusy = true;
            var items = courses as Course[] ?? 
                        courses?.Cast<Course>().ToArray() ?? 
                    throw new ArgumentNullException(nameof(courses));
            foreach (var item in items)
                await _data.StudentsRep.SetCourseAsync(SelectedStudent!, item);
            
            SetInOutCourses();
            AdvancedOutCourses?.SelectItem();
            AdvancedInCourses?.SelectItems(items);
            StudentsRefreshable?.Refresh();
            CoursesRefreshable?.Refresh();
        }
        finally
        {
            _isBusy = false;
            AsyncSetCoursesCommand.RaiseCanExecuteChanged();
            AsyncUnsetCoursesCommand.RaiseCanExecuteChanged();
        }
    }
    private async Task UnsetCourseAsync(IEnumerable? courses)
    {
        _isBusy = true;
        try
        {
            _isBusy = true;
            var items = courses as Course[] ?? 
                        courses?.Cast<Course>().ToArray() ??
                throw new ArgumentNullException(nameof(courses));
            foreach (var item in items)
                await _data.StudentsRep.UnsetCourseAsync(SelectedStudent!, item);

            SetInOutCourses();
            AdvancedInCourses?.SelectItem();
            AdvancedOutCourses?.SelectItems(items);
            StudentsRefreshable?.Refresh();
            CoursesRefreshable?.Refresh();
        }
        finally
        {
            _isBusy = false;
            AsyncUnsetCoursesCommand.RaiseCanExecuteChanged();
            AsyncSetCoursesCommand.RaiseCanExecuteChanged();
        }
    }
    #endregion

    #region selected
    private Student? _selectedStudent;
    public Student? SelectedStudent
    {
        get => _selectedStudent;
        set
        {
            if (!set(ref _selectedStudent, value)) return;
            SetInOutCourses();
            if (SelectedCourse is not null)
            {
                AdvancedInCourses?.SelectItem(SelectedCourse);
                AdvancedOutCourses?.SelectItem(SelectedCourse);
            }
            AsyncStudentDeleteCommand.RaiseCanExecuteChanged();
            AsyncSetCoursesCommand.RaiseCanExecuteChanged();
            AsyncSetCoursesCommand.RaiseCanExecuteChanged();
        }
    }

    private void SetInOutCourses()
    {
        InCourses = SelectedStudent is null
            ? null
            : new ObservableCollection<Course>(
                _data.CoursesRep.Items.Where(c => c.Students.Contains(SelectedStudent)));
        OnPropertyChanged(nameof(InCourses));

        OutCourses = SelectedStudent is null
            ? null
            : new ObservableCollection<Course>(
                _data.CoursesRep.Items.Where(c => !c.Students.Contains(SelectedStudent)));
        OnPropertyChanged(nameof(OutCourses));
    }

    private Course? _selectedCourse;
    public Course? SelectedCourse
    {
        get => _selectedCourse;
        set
        {
            if (!set(ref _selectedCourse, value)) return;
            AsyncCourseDeleteCommand.RaiseCanExecuteChanged();
            if (value is not null)
            {
                AdvancedInCourses?.SelectItem(value);
                AdvancedOutCourses?.SelectItem(value);
            }
            AsyncSetCoursesCommand.RaiseCanExecuteChanged();
            AsyncSetCoursesCommand.RaiseCanExecuteChanged();
        }
    }
    #endregion

    #region weather
    private byte[]? weatherImage;

    public byte[] WeatherImage
    {
        get
        {
            //if (weatherImage is not null) return weatherImage;
            //using var httpClient = new HttpClient();
            //var url = $"http://openweathermap.org/img/wn/10d@2x.png";
            //weatherImage = httpClient.GetByteArrayAsync(url).Result;

            return weatherImage;
        }
    }


    #endregion
}