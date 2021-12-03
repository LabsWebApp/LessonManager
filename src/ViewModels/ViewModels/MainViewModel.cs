using System.Collections;
using Models;
using Models.Entities;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Input;
using ApiKeysData;
using GetLocation;
using Models.DataProviders.Helpers;
using OpenWeatherMapApi;
using OpenWeatherMapApi.Infos;
using ViewModelBase.Commands;
using ViewModelBase.Commands.AsyncCommands;
using ViewModelBase.Commands.ErrorHandlers;
using ViewModelBase.Commands.QuickCommands;
using ViewModels.Interfaces;

namespace ViewModels;

public class MainViewModel : ViewModelBase.ViewModelBase, IDisposable
{
    #region init
    private readonly DataManager _data;
    private const string PatternName = @"^[А-ЯЁ][а-яё]";
    private const string SyncErrMsg = @"База данных была изменена извне - перегрузите её.";
    private bool _isBusy;
    private readonly IConfirmed _confirmed;

    public IRefreshable? StudentsRefreshable { private get; set; } 
    public IRefreshable? CoursesRefreshable { private get; set; } 
    public IAdvancedSelectedItems? AdvancedInCourses { private get; set; } 
    public IAdvancedSelectedItems? AdvancedOutCourses { private get; set; } 
    public ObservableCollection<ProxyEntity> ProxyStudents { get; set; }
    public ObservableCollection<ProxyEntity> ProxyCourses { get; set; }
    public ObservableCollection<ProxyEntity>? OutProxyCourses { get; set; }
    public ObservableCollection<ProxyEntity>? InProxyCourses { get; set; }

    public MainViewModel(Provider provider, IErrorHandler errorHandler, IConfirmed confirmed)
    {
        //_errorHandler = errorHandler;
        _confirmed = confirmed;
        _data = DataManager.Get(provider);
        ProxyStudents = new ObservableCollection<ProxyEntity>(_data.StudentsRep.ProxyItems);
        ProxyCourses = new ObservableCollection<ProxyEntity>(_data.CoursesRep.ProxyItems);

        AsyncStudentCreateCommand = new AsyncCommand(CreateStudentAsync, () => CanCreate(_newStudent), errorHandler);
        AsyncCourseCreateCommand = new AsyncCommand(CreateCourseAsync, () => CanCreate(_newCourse), errorHandler);
        StudentFindCommand = new Command(StudentFind,
            () => ProxyStudents.Any() && _findStudent.Trim().Length > 0, errorHandler);
        CourseFindCommand = new Command(CourseFind,
            () => ProxyCourses.Any() && _findCourse.Trim().Length > 0, errorHandler);
        AsyncStudentDeleteCommand = new AsyncCommand(StudentDeleteAsync, 
            () => !_isBusy && SelectedProxyStudent is not null,errorHandler);
        AsyncCourseDeleteCommand = new AsyncCommand(CourseDeleteAsync,
            () => !_isBusy && SelectedProxyCourse is not null, errorHandler);
        AsyncSetCoursesCommand = new AsyncCommand<IEnumerable?>(
            SetCoursesAsync,
            CanSetUnset, errorHandler);
        AsyncUnsetCoursesCommand = new AsyncCommand<IEnumerable?>(
            UnsetCourseAsync,
            CanSetUnset,
            errorHandler);
        AsyncStudentNameChangeCommand = new AsyncCommand<string>(
            StudentNameChangeAsync,
            s => s != null && CanCreate(s) && SelectedProxyStudent != null && SelectedProxyStudent.Name != ChangingStudent,
            errorHandler);

        _asyncBonusCommand = new AsyncCommand(BonusAsync, null, errorHandler, _infoCts.Token);
        AsyncRefreshBonusCommand = new AsyncCommand(RefreshBonusAsync,
            () => _isBonusBusy && !_isRefreshRunning, 
            errorHandler, _infoCts.Token);
        BonusCommand = new Command(Bonus, null, errorHandler);
    }

    private Student? StudentByProxy(ProxyEntity? proxy) =>
        proxy == null ? null : _data.StudentsRep.GetStudentByProxy(proxy);
    private Course? CourseByProxy(ProxyEntity proxy) => _data.CoursesRep.GetCourseByProxy(proxy);

    private void RefreshProxies(ObservableCollection<ProxyEntity> items, IList<Guid> ids)
    {
        if (!ids.Any() || !items.Any()) return;

        var isCourses = ProxyCourses.Any(c => c.Id == ids.First());

        //foreach (var id in ids)
        //{
        //    var entity = 
        //}
    }

    #endregion

    #region rename
    public AsyncCommand<string> AsyncStudentNameChangeCommand { get; }

    private string? _changingStudent;
    public string? ChangingStudent
    {
        get => _changingStudent;
        set
        {
            if (set(ref _changingStudent, value))
                AsyncStudentNameChangeCommand.RaiseCanExecuteChanged();
        }
    }

    private async Task StudentNameChangeAsync(string? obj)
    {
        if (obj == null) return;
        try
        {
            _isBusy = true;
            ChangingStudent = obj.TrimEnd();
            var student = StudentByProxy(SelectedProxyStudent) ??
                          throw new SynchronizationLockException(SyncErrMsg);
            await _data.StudentsRep.RenameAsync(student, ChangingStudent);
            //
            //student.Name = ChangingStudent;
            SelectedProxyStudent.Name = ChangingStudent!;
            var ss = ProxyStudents
                .FirstOrDefault(s => s.Id == SelectedProxyStudent.Id);
            var res = ProxyStudents.IndexOf(ss);
            if (res < 0) throw new Exception();
            ProxyStudents[res] = SelectedProxyStudent;
            OnPropertyChanged(nameof(SelectedProxyStudent));
            //StudentsRefreshable?.Refresh();
        }
        finally { _isBusy = false; }
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
            ProxyStudents.Insert(0, new ProxyEntity(student));
            SelectedProxyStudent = new(student);
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
            ProxyCourses.Insert(0, new(course));
            SelectedProxyCourse = new(course);
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
        var index = SelectedProxyStudent is null ? 0 : ProxyStudents.IndexOf(SelectedProxyStudent);
        var result = ProxyStudents
            .Skip(index + 1)
            .Union(ProxyStudents.Take(index + 1))
            .FirstOrDefault(s => s.Name.ToLower().Contains(_findStudent.ToLower()));
        SelectedProxyStudent = result == default ? throw new ResultNotFoundException(_findStudent) : result;
    }
    private void CourseFind()
    {
        CoursesRefreshable?.Refresh();
        var index = SelectedProxyCourse is null ? 0 : ProxyCourses.IndexOf(SelectedProxyCourse);
        var result = ProxyCourses
            .Skip(index + 1)
            .Union(ProxyCourses.Take(index + 1))
            .FirstOrDefault(s => s.Name.ToLower().Contains(_findCourse.ToLower()));
        SelectedProxyCourse = result == default ? throw new ResultNotFoundException(_findCourse) : result;
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
            if (_confirmed.Confirm($"Вы действительно хотите отчислит студента: {SelectedProxyStudent?.Name}?"))
                throw new OperationCanceledException("Отчисление отменено.");
            if (SelectedProxyStudent is not null)
            {
                await _data.StudentsRep.DeleteAsync(SelectedProxyStudent.Id);
                ProxyStudents.Remove(SelectedProxyStudent);
            }
            SelectedProxyStudent = null;
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
            if (_confirmed.Confirm($"Вы действительно хотите удалить курс: \"{SelectedProxyCourse?.Name}\"?"))
                throw new OperationCanceledException("Удаление отменено.");
            if (SelectedProxyCourse is not null)
            {
                await _data.CoursesRep.DeleteAsync(SelectedProxyCourse.Id);
                ProxyCourses.Remove(SelectedProxyCourse);
            }
            SelectedProxyCourse = null;
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
        !_isBusy && (obj?.Cast<ProxyEntity>().Any() ?? false);

    private async Task SetCoursesAsync(IEnumerable? courses)
    {
        _isBusy = true;
        try
        {
            _isBusy = true;
            var items = courses as ProxyEntity[] ?? 
                        courses?.Cast<ProxyEntity>().ToArray() ?? 
                    throw new ArgumentNullException(nameof(courses));
            foreach (var item in items)
                await _data.StudentsRep.SetCourseAsync(
                    StudentByProxy(SelectedProxyStudent) ?? throw new SynchronizationLockException("SyncErrMsg"), 
                    CourseByProxy(item) ?? throw new SynchronizationLockException("SyncErrMsg"));
            
            SetInOutCourses();
            AdvancedOutCourses?.SelectItem();
            AdvancedInCourses?.SelectItems(items.Cast<object>());
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
            var items = courses as ProxyEntity[] ?? 
                        courses?.Cast<ProxyEntity>().ToArray() ??
                throw new ArgumentNullException(nameof(courses));
            foreach (var item in items)
                await _data.StudentsRep.UnsetCourseAsync(
                    StudentByProxy(SelectedProxyStudent) ?? throw new SynchronizationLockException(SyncErrMsg),
                    CourseByProxy(item) ?? throw new SynchronizationLockException(SyncErrMsg));

            SetInOutCourses();
            AdvancedInCourses?.SelectItem();
            AdvancedOutCourses?.SelectItems(items.Cast<object>());
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
    private ProxyEntity? _selectedProxyStudent;
    public ProxyEntity? SelectedProxyStudent
    {
        get => _selectedProxyStudent;
        set
        {
            if (!set(ref _selectedProxyStudent, value)) return;
            ChangingStudent = value?.Name;
            SetInOutCourses();
            if (SelectedProxyCourse is not null)
            {
                AdvancedInCourses?.SelectItem(SelectedProxyCourse);
                AdvancedOutCourses?.SelectItem(SelectedProxyCourse);
            }
            AsyncStudentDeleteCommand.RaiseCanExecuteChanged();
        }
    }

    private void SetInOutCourses()
    {
        var student = StudentByProxy(SelectedProxyStudent);
        InProxyCourses = student is null
            ? null
            : new ObservableCollection<ProxyEntity>(
                _data.CoursesRep.Items
                    .Where(c => c.Students.Contains(student))
                    .Select(c => new ProxyEntity(c)));
        OnPropertyChanged(nameof(InProxyCourses));

        OutProxyCourses = student is null
            ? null
            : new ObservableCollection<ProxyEntity>(
                _data.CoursesRep.Items
                    .Where(c => !c.Students.Contains(student))
                    .Select(c => new ProxyEntity(c)));
        OnPropertyChanged(nameof(OutProxyCourses));
    }

    private ProxyEntity? _selectedProxyCourse;
    public ProxyEntity? SelectedProxyCourse
    {
        get => _selectedProxyCourse;
        set
        {
            if (!set(ref _selectedProxyCourse, value)) return;
            AsyncCourseDeleteCommand.RaiseCanExecuteChanged();
            if (value is null) return;
            AdvancedInCourses?.SelectItem(value);
            AdvancedOutCourses?.SelectItem(value);
        }
    }
    #endregion

    #region weather

    private readonly AsyncCommand _asyncBonusCommand;
    public AsyncCommand AsyncRefreshBonusCommand { get;}
    public Command BonusCommand { get; }

    private CancellationTokenSource _infoCts = new ();
    private bool _isBonusBusy, _isRefreshRunning;

    private string? _ipGeolocationDotIoKey;
    private string? _openWeatherMapDotOrgKey;
    private readonly Stopwatch _stopwatch = new();

    private string? _timeString;
    public string? TimeString
    {
        get => _timeString;
        set => set(ref _timeString, value);
    }

    private byte[]? _weatherImage = Icons._10d;
    public byte[]? WeatherImage
    {
        get => _weatherImage;
        set => set(ref _weatherImage, value);
    }

    private string? _weatherString;
    public string? WeatherString
    {
        get => _weatherString;
        set => set(ref _weatherString, value);
    }

    private string? _weatherToolTipsString;
    public string? WeatherToolTipsString
    {
        get => _weatherToolTipsString;
        set => set(ref _weatherToolTipsString, value);
    }

    private void SetTime() => TimeString = DateTime.Now.ToLongTimeString();

    private static void SetCancelException(CancellationToken ct)
    {
        if (ct.IsCancellationRequested)
            throw new OperationCanceledException("Показ бонусов прерван пользователем.");
    }

    private async Task SetWeatherAsync(CancellationToken ct)
    {
        if (_ipGeolocationDotIoKey is null || _openWeatherMapDotOrgKey is null)
        {
            var keys = await ApiKeys.GetAllAsync();
            SetCancelException(ct);
            _ipGeolocationDotIoKey = keys
                .FirstOrDefault(k => k.Provider == ApiKeyProvider.IpGeolocationDotIo)
                .Key ?? throw new Exception("Не найден ключ к IpGeolocationDotIo.");
            _openWeatherMapDotOrgKey = keys
                .FirstOrDefault(k => k.Provider == ApiKeyProvider.OpenWeatherMapDotOrg)
                .Key ?? throw new Exception("Не найден ключ к OpenWeatherMapDotOrg.");
        }

        var ip = await Ip.GetIpIfyDotOrgAsync(ct);
        SetCancelException(ct);
        if (ip == null) throw new Exception("Поставщик ip не отвечает.");

        var (lat, lon) = await GetGeolocation.GetAsync(_ipGeolocationDotIoKey, ip, ct);
        if (lat.StartsWith('-')) throw new HttpRequestException("Поставщик погоды не отвечает.");
        SetCancelException(ct);

        var result = await WeatherClient.QueryAsync(lat, lon, _openWeatherMapDotOrgKey, ct);
        if (result is null) throw new HttpRequestException("Поставщик погоды не отвечает.");
        SetCancelException(ct);

        WeatherImage = await result.WeatherList[0].GetIcon(ct);
        SetCancelException(ct);
        WeatherToolTipsString =
            $"{result.Name}\n{result.WeatherList[0].Description}\nвосход: {result.Sys?.Sunrise.ToShortTimeString()} закат: {result.Sys?.Sunset.ToShortTimeString()}\n(2ой клик обновить сейчас)";
        WeatherString = result.Main?.Temperature.CelsiusCurrent.ToString(CultureInfo.CurrentCulture) +
                        TemperatureObject.Degree + ", " +
                        result.Main?.Humidity.ToString(CultureInfo.CurrentCulture) + Main.HumidityRu + ", " +
                        result.Main?.Pressure.PressureMmHg.ToString(CultureInfo.CurrentCulture) +
                        PressureObject.MmHgRu + ", " +
                        result.Wind?.SpeedMetersPerSecond.ToString(CultureInfo.CurrentCulture)
                        + Wind.DirectionEnumToStringRu(result.Wind?.Direction ?? DirectionEnum.Unknown) +
                        $" [{DateTime.Now.ToShortTimeString()}]";
    }

    private async Task RefreshBonusAsync(CancellationToken ct)
    {
        try
        {
            _isRefreshRunning = true;
            _stopwatch.Stop();
            await SetWeatherAsync(ct);
            _stopwatch.Restart();
        }
        finally
        {
            _isRefreshRunning = false;
            AsyncRefreshBonusCommand.RaiseCanExecuteChanged();
        }
    }

    private const string Show = "Погода",
        Cancel = "Отмена",
        ShowTip = "показать информацию о погоде",
        CancelTip = "отключить обновление и показ доп. информации";

    private string _buttonText = Show;
    public string ButtonText
    {
        get => _buttonText;
        private set => set(ref _buttonText, value);
    }

    private string _buttonToolTip = ShowTip;
    public string ButtonToolTip
    {
        get => _buttonToolTip;
        private set => set(ref _buttonToolTip, value);
    }

    private void Bonus()
    {
        if (!_isBonusBusy)
        {
            ((ICommand)_asyncBonusCommand).Execute(null);
            return;
        }

        _infoCts.Cancel();
        TimeString = string.Empty;
        WeatherImage = Icons._10d;
        WeatherString = string.Empty;
        WeatherToolTipsString = string.Empty;
        ButtonText = Show;
        ButtonToolTip = ShowTip;
        _infoCts.Dispose();
        _infoCts = new();
        AsyncRefreshBonusCommand.ResetCancel(ref _infoCts);
        _asyncBonusCommand.ResetCancel(ref _infoCts);
    }

    private async Task BonusAsync(CancellationToken ct)
    {
        if (!Ip.HasConnection())
            throw new Exception("Проверьте соединения с глобальной сетью.");
        _isBonusBusy = true;
        WeatherString = "Собираем информацию о погоде рядом с Вами...";
        ButtonText = Cancel;
        ButtonToolTip = CancelTip;
        try
        {
            _stopwatch.Restart();
            var notFirst = false;
            while (true)
            {
                SetTime();
                await Task.Delay(1000, ct);
                if (ct.IsCancellationRequested)
                    throw new OperationCanceledException("-1");
                if (_stopwatch.Elapsed.Minutes < 5 && notFirst) continue;
                notFirst = true;
                (AsyncRefreshBonusCommand as ICommand).Execute(null);
            }
        }
        finally
        {
            _isBonusBusy = false;
            ButtonText = Show;
            ButtonToolTip = ShowTip;
        }
    }
    #endregion

    #region IDisposable
    private bool _disposedValue;
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                _infoCts.Dispose();
            }

            // TODO: освободить неуправляемые ресурсы (неуправляемые объекты) и переопределить метод завершения
            // TODO: установить значение NULL для больших полей
            _disposedValue = true;
        }
    }

    // // TODO: переопределить метод завершения, только если "Dispose(bool disposing)" содержит код для освобождения неуправляемых ресурсов
    // ~MainViewModel()
    // {
    //     // Не изменяйте этот код. Разместите код очистки в методе "Dispose(bool disposing)".
    //     Dispose(disposing: false);
    // }

    public void Dispose()
    {
        // Не изменяйте этот код. Разместите код очистки в методе "Dispose(bool disposing)".
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
    #endregion
}