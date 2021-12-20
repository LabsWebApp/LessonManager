using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Input;
using ApiKeysData;
using GetLocation;
using Models;
using Models.DataProviders.Helpers;
using Models.Entities;
using Models.Entities.Proxies;
using OpenWeatherMapApi;
using OpenWeatherMapApi.Infos;
using ViewModelBase.Commands;
using ViewModelBase.Commands.AsyncCommands;
using ViewModelBase.Commands.ErrorHandlers;
using ViewModelBase.Commands.QuickCommands;
using ViewModels.Helpers;
using ViewModels.Interfaces;

namespace ViewModels;

public class MainViewModel : ViewModelBase.ViewModelBase, IDisposable
{
    #region init
    private readonly DataManager _data;
    private const string PatternName = @"^[А-ЯЁ][а-яё]";
    private readonly IConfirmed _confirmed;
    private readonly Action<object?> _onSyncException;
    private readonly IWarningHandler? _warning;

    public readonly object ItemLock = new();
    public ObservableUniqueCollection<ProxyEntity> ProxyStudents { get; } = new();
    public ObservableUniqueCollection<ProxyCourse> ProxyCourses { get; } = new();
    public ObservableUniqueCollection<ProxyCourse> OutProxyCourses { get; } =
        new(new IdProxyEntityComparer());
    public ObservableUniqueCollection<ProxyCourse> InProxyCourses { get; } =
        new(new IdProxyEntityComparer());
    public ObservableUniqueCollection<ProxyCourse> SelectedProxyCourses { get; set; } =
        new(new IdProxyEntityComparer());

    public MainViewModel(Provider provider, IErrorHandler errorHandler, IConfirmed confirmed)
    {
        _confirmed = confirmed;
        _data = DataManager.Get(provider);
        _onSyncException = msg => InfoStr = msg?.ToString();
        _warning = errorHandler as IWarningHandler;

        StudentCreateAsyncCommand = new AsyncCommand(CreateStudentAsync, () => CanCreate(_newStudent), errorHandler);
        CourseCreateAsyncCommand = new AsyncCommand(CreateCourseAsync, () => CanCreate(_newCourse), errorHandler);
        StudentFindCommand = new Command(StudentFind,
            () => ProxyStudents.Any() && _findStudent.Trim().Length > 0, errorHandler);
        CourseFindCommand = new Command(CourseFind,
            () => ProxyCourses.Any() && _findCourse.Trim().Length > 0, errorHandler);
        StudentDeleteAsyncCommand = new AsyncCommand(StudentDeleteAsync, 
            () => !IsBusy && SelectedProxyStudent is not null,errorHandler);
        CourseDeleteAsyncCommand = new AsyncCommand(CourseDeleteAsync,
            () => !IsBusy && SelectedProxyCourse is not null, errorHandler);
        SetCoursesAsyncCommand = new AsyncCommand<IEnumerable?>(
            SetCoursesAsync,
            items => CanSetUnset(items, OutProxyCourses), 
            errorHandler);
        UnsetCoursesAsyncCommand = new AsyncCommand<IEnumerable?>(
            UnsetCourseAsync,
            items => CanSetUnset(items, InProxyCourses),
            errorHandler);
        StudentNameChangeAsyncCommand = new AsyncCommand<string>(
            StudentNameChangeAsync,
            s => s != null 
                 && CanCreate(s)
                 && SelectedProxyStudent != null
                 && SelectedProxyStudent.Name != ChangingStudent?.Trim(),
            errorHandler);

        _reloadCts = new CancellationTokenSource();
        _progressLoadingAsyncCommand = new AsyncCommand(SetProgressingAsync,cancel: _reloadCts.Token);
        var reloadingAsyncCommand = new AsyncCommand(AllDataReloadingAsync, errorCancelHandler: errorHandler);
        ReloadingCommand = new Command(() =>
        {
            ((ICommand)_progressLoadingAsyncCommand).Execute(null);
            ((ICommand)reloadingAsyncCommand).Execute(null);
        }, () => !IsBusy);
        ClearSelectionsCommand = new Command(ClearSelections);
        ((ICommand)ReloadingCommand).Execute(null);

        _bonusAsyncCommand = new AsyncCommand(BonusAsync, null, errorHandler, _infoCts.Token);
        RefreshBonusAsyncCommand = new AsyncCommand(RefreshBonusAsync,
            () => _isBonusBusy && !_isRefreshRunning, 
            errorHandler, _infoCts.Token);
        BonusCommand = new Command(Bonus, null, errorHandler);

        SelectedProxyCourses.CollectionChanged += RaiseSetUnsetCanExecuteChanged;
    }
    #endregion

    #region reloading
    public Command ReloadingCommand { get; }
    private bool _isBusy;
    private const string OkString = "Студенты на курсах [готово]"; 
    private const string LoadString = "Подождите, идёт загрузка данных";
    private CancellationTokenSource? _reloadCts;
    
    private readonly AsyncCommand _progressLoadingAsyncCommand;
    private async Task SetProgressingAsync(CancellationToken ct)
    {
        do
        {
            InfoStr = InfoStr?.EndsWith(" . . . . . . . . .") ?? true 
                ? LoadString : InfoStr + " .";
            await Task.Delay(100, ct);
            if (ct.IsCancellationRequested) 
                InfoStr = OkString;
        } while (InfoStr != OkString);
    }

    private bool IsBusy
    {
        get => _isBusy;
        set
        {
            if (!Set(ref _isBusy, value)) return;
            StudentCreateAsyncCommand.RaiseCanExecuteChanged();
            StudentNameChangeAsyncCommand.RaiseCanExecuteChanged();
            CourseCreateAsyncCommand.RaiseCanExecuteChanged();
            StudentDeleteAsyncCommand.RaiseCanExecuteChanged();
            CourseDeleteAsyncCommand.RaiseCanExecuteChanged();
            ReloadingCommand.RaiseCanExecuteChanged();
            //RaiseSetUnsetCanExecuteChanged();
        }
    }

    private async Task ReloadingAsync<TProxy>(
        IQueryable<TProxy> entities, 
        ICollection<TProxy> items, 
        SelectedComparer<TProxy>? comparer = null)
        where TProxy : ProxyEntity
    {
        //items.Clear();
        var getData = Task.Factory.StartNew(() => entities);
        await getData.ContinueWith(t =>
        {
            dynamic data = comparer is null
                ? t.Result
                : t.Result.ToList().OrderBy(x => x.Id, comparer);
            try
            {
                lock (ItemLock)
                {
                    foreach (var item in data)
                        items.Add(item);
                }
            }
            catch (Exception ex)
            {
                _warning?.ShowWarning($"Возникла ошибка перезагрузки, возможно данные заняты другими задачами:\n    \"{ex.Message}\"", false);
            }
        }/*, TaskContinuationOptions.AttachedToParent*/);
    }

    private async Task AllDataReloadingAsync(CancellationToken _)
    {
        try
        {
            IsBusy = true;
            InfoStr = LoadString;

            var oldSelectedStudent = SelectedProxyStudent;
            var oldSelectedCourse = SelectedProxyCourse;
            var oldSelectedCourses = new ProxyCourse[SelectedProxyCourses.Count];
            SelectedProxyCourses.CopyTo(oldSelectedCourses, 0);
            var mustRemove = oldSelectedCourse != null 
                               && !SelectedProxyCourses.Contains(oldSelectedCourse);

            ProxyStudents.Clear();
            ProxyCourses.Clear();
            SelectedProxyCourses.Clear();
            InProxyCourses.Clear();
            OutProxyCourses.Clear();
            SelectedProxyCourse = null;
            SelectedProxyStudent = null;

            await ReloadingAsync(_data.StudentsRep.ProxyItems, ProxyStudents);
            await ReloadingAsync(_data.CoursesRep.ProxyItems, ProxyCourses);

            InfoStr = OkString;

            bool studentNotCorrect = false, coursesNotCorrect = false;

            foreach (var item in oldSelectedCourses)
            {
                if (ProxyCourses.Contains(item)) 
                    SelectedProxyCourses.Add(item);
                else
                {
                    coursesNotCorrect = true;
                    var addCourse = ProxyCourses.FirstOrDefault(
                        c => c.Id == item.Id);
                    if (addCourse is not null) AddSelectedCourse(addCourse);
                }
            }

            if (oldSelectedCourse is not null)
            {
                var addCourse = ProxyCourses.FirstOrDefault(
                    c => c.Id == oldSelectedCourse.Id);
                if (addCourse is not null)
                {
                    SelectedProxyCourse = addCourse;
                    if (mustRemove)
                    {
                        SelectedProxyCourses.Remove(addCourse);
                        InProxyCourses.Remove(addCourse);
                        OutProxyCourses.Remove(addCourse);
                    }
                }
            }
            if (oldSelectedStudent is not null)
            {
                if (!ProxyStudents.Contains(oldSelectedStudent))
                {
                    studentNotCorrect = true;
                    var addSelectedStudent = ProxyStudents.FirstOrDefault(
                        c => c.Id == oldSelectedStudent.Id);
                    if (addSelectedStudent is not null)
                        SelectedProxyStudent = addSelectedStudent;
                }
                else SelectedProxyStudent = oldSelectedStudent;
            }

            if (studentNotCorrect || coursesNotCorrect)
            {
                var errorMsg = "В результате перезагрузки обнаружено, что прерванная работа может быть не совсем корректно продолжена:";
                if (studentNotCorrect)
                    errorMsg += "\n    - \"Студент в работе\" был изменён или удалён";
                if (coursesNotCorrect)
                    errorMsg += "\n    - выбранный(е) для работы курс(ы) был(и) изменен(ы) или удален(ы)";
                throw new WarningException(errorMsg);
            }
        }
        finally
        {
            _reloadCts?.Cancel();
            _progressLoadingAsyncCommand.ResetCancel(ref _reloadCts!);
            IsBusy = false;
        }
    }

    private string? _infoStr;
    public string? InfoStr
    {
        get => _infoStr;
        set => Set(ref _infoStr, value);
    }
    #endregion

    #region rename
    public AsyncCommand<string> StudentNameChangeAsyncCommand { get; }

    private string? _changingStudent;
    public string? ChangingStudent
    {
        get => _changingStudent;
        set
        {
            if (Set(ref _changingStudent, value))
                StudentNameChangeAsyncCommand.RaiseCanExecuteChanged();
        }
    }

    private async Task StudentNameChangeAsync(string? obj)
    {
        if (obj == null) return;
        try
        {
            IsBusy = true;
            ChangingStudent = obj.TrimEnd();
            var student = await _data.StudentsRep.GetStudentByIdAsync(
                              SelectedProxyStudent?.Id ?? 
                              throw new SynchronizationDataException(_onSyncException)) ?? 
                          throw new SynchronizationDataException(_onSyncException);
            var selected = await _data.StudentsRep.RenameAsync(student, ChangingStudent);
            var proxy = new ProxyStudent(selected ?? throw new SynchronizationDataException(_onSyncException));
            SelectedProxyStudent = await ProxyStudents.Refresh(_data, proxy, _onSyncException);
        }
        finally
        {
            IsBusy = false;
        }
    }
    #endregion

    #region create
    private bool CanCreate(string name)
    {
        if (IsBusy) return false;
        name = name.Trim();
        return !string.IsNullOrEmpty(name) && Regex.IsMatch(name, PatternName);
    }

    private string _newStudent = string.Empty;
    public string NewStudent
    {
        set
        {
            if (Set(ref _newStudent, value))
                StudentCreateAsyncCommand.RaiseCanExecuteChanged();
        }
    }

    private string _newCourse = string.Empty;
    public string NewCourse
    {
        set
        {
            if (!Set(ref _newCourse, value)) return;
            CourseCreateAsyncCommand.RaiseCanExecuteChanged();
            SetInOutCourses();
        }
    }
    public AsyncCommand StudentCreateAsyncCommand { get; }
    private async Task CreateStudentAsync()
    {
        IsBusy = true;
        try
        {
            Student student = new() { Name = _newStudent };
            await _data.StudentsRep.AddAsync(student);
            ProxyStudents.Insert(0, new ProxyStudent(student));
            SelectedProxyStudent = new ProxyStudent(student);
            NewStudent = string.Empty;
        }
        finally
        {
            IsBusy = false;
        }
    }

    public AsyncCommand CourseCreateAsyncCommand { get; }
    private async Task CreateCourseAsync()
    {
        IsBusy = true;
        try
        {
            Course course = new() { Name = _newCourse };
            await _data.CoursesRep.AddAsync(course);
            var proxy = new ProxyCourse(course);
            ProxyCourses.Insert(0, proxy);
            SelectedProxyCourse = proxy;
            NewCourse = string.Empty;
        }
        finally
        {
            IsBusy = false;
        }
    }
    #endregion

    #region find
    private string _findStudent = string.Empty;
    public string FindStudent
    {
        set
        {
            if (Set(ref _findStudent, value))
                StudentFindCommand.RaiseCanExecuteChanged();
        }  
    }

    private string _findCourse = string.Empty;
    public string FindCourse
    {
        set
        {
            if (Set(ref _findCourse, value))
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
        SelectedProxyStudent = result ?? throw new ResultNotFoundException(_findStudent);
    }
    private void CourseFind()
    {
        var index = SelectedProxyCourse is null ? 0 : ProxyCourses.IndexOf(SelectedProxyCourse);
        var result = ProxyCourses
            .Skip(index + 1)
            .Union(ProxyCourses.Take(index + 1))
            .FirstOrDefault(s => s.Name.ToLower().Contains(_findCourse.ToLower()));
        SelectedProxyCourse = result ?? throw new ResultNotFoundException(_findCourse);
    }
    #endregion

    #region delete
    public AsyncCommand StudentDeleteAsyncCommand { get; }
    public AsyncCommand CourseDeleteAsyncCommand { get; }

    private async Task StudentDeleteAsync()
    {
        IsBusy = true;
        try
        {
            if (!_confirmed.Confirm($"Вы действительно хотите отчислит студента: {SelectedProxyStudent?.Name}?"))
                throw new OperationCanceledException("Отчисление отменено.");

            var changedCourses = _data.CoursesRep.Items
                .Where(c => c.Students.Any(s => s.Id == SelectedProxyStudent!.Id))
                .Select(c => c.Id).ToList();
            if (SelectedProxyStudent is not null)
            {
                await _data.StudentsRep.DeleteAsync(SelectedProxyStudent.Id);
                ProxyStudents.Remove(SelectedProxyStudent);
            }

            changedCourses.ForEach(id =>
            {
                var item = ProxyCourses.FirstOrDefault(c => c.Id == id);
                if (item == null) return;
                ProxyCourses[ProxyCourses.IndexOf(item)] = item with { Count = item.Count - 1 };
            });
            //CoursesRefreshable?.Refresh();
            SelectedProxyStudent = null;
        }
        finally
        {
            IsBusy = false;
            SetInOutCourses();
        }
    }
    private async Task CourseDeleteAsync()
    {
        IsBusy = true;
        try
        {
            if (!_confirmed.Confirm($"Вы действительно хотите удалить курс: \"{SelectedProxyCourse?.Name}\"?"))
                throw new OperationCanceledException("Удаление отменено.");

            var changedStudents = _data.StudentsRep.Items
                .Where(c => c.Courses.Any(c => c.Id == SelectedProxyCourse!.Id))
                .Select(c => c.Id).ToList();
            if (SelectedProxyCourse is not null)
            {
                await _data.CoursesRep.DeleteAsync(SelectedProxyCourse.Id);
                ProxyCourses.Remove(SelectedProxyCourse);
                SelectedProxyCourses.Remove(SelectedProxyCourse);
                SelectedProxyCourse = null;
            }

            changedStudents.ForEach(id =>
            {
                var item = ProxyStudents.FirstOrDefault(s => s.Id == id);
                if (item == null) return;
                ProxyStudents[ProxyStudents.IndexOf(item)] = item with { Count = item.Count - 1 };
            });
            //StudentsRefreshable?.Refresh();
        }
        finally
        {
            IsBusy = false;
            SetInOutCourses();
        }
    }
    #endregion

    #region set
    public AsyncCommand<IEnumerable?> SetCoursesAsyncCommand { get; }
    public AsyncCommand<IEnumerable?> UnsetCoursesAsyncCommand { get; }

    private bool CanSetUnset(IEnumerable? obj, ObservableUniqueCollection<ProxyCourse> courses) => 
        SelectedProxyStudent is not null
        && courses.IsIntersect(SelectedProxyCourses);

    private void RaiseSetUnsetCanExecuteChanged(object? o = null, NotifyCollectionChangedEventArgs? e = null)
    {
        UnsetCoursesAsyncCommand.RaiseCanExecuteChanged();
        SetCoursesAsyncCommand.RaiseCanExecuteChanged();
    }

    private async Task<Student?> StudentByProxyAsync(ProxyEntity? proxy) =>
        proxy == null ? null : await _data.StudentsRep.GetStudentByIdAsync(proxy.Id);
    private async Task<Course?> CourseByProxyAsync(ProxyEntity? proxy) =>
        proxy == null ? null : await _data.CoursesRep.GetCourseByIdAsync(proxy.Id);

    private async Task SetCoursesAsync(IEnumerable? courses) =>
        await SetUnsetCourseAsync(courses, _data.StudentsRep.SetCourseAsync);

    private async Task UnsetCourseAsync(IEnumerable? courses) =>
        await SetUnsetCourseAsync(courses, _data.StudentsRep.UnsetCourseAsync);

    private async Task SetUnsetCourseAsync(
        IEnumerable? courses, 
        Func<Student,Course,CancellationToken, Task> setUnset)
    {
        IsBusy = true;
        try
        {
            IsBusy = true;
            var items = courses as ProxyCourse[] ??
                        courses?.Cast<ProxyCourse>().ToArray() ??
                        throw new ArgumentNullException(nameof(courses));
            var student = await _data.StudentsRep
                              .GetStudentByIdAsync(SelectedProxyStudent!.Id) 
                          ?? throw new SynchronizationDataException(_onSyncException);

            var oldSelectedId = SelectedProxyCourse?.Id;
            SelectedProxyCourses.Clear();
            foreach (var item in items.Reverse())
            {
                await setUnset(
                    student ??
                    throw new SynchronizationLockException(),
                    await CourseByProxyAsync(item) ??
                    throw new SynchronizationDataException(_onSyncException),
                    default);
                var result = await ProxyCourses.Refresh(_data, item, _onSyncException);
                while (!ProxyCourses.Contains(result))
                {
                    await Task.Delay(1);
                }

                if (oldSelectedId is not null && item.Id == oldSelectedId)
                    SelectedProxyCourse = result;
                else
                    SelectedProxyCourses.Add(result);
            }
            IsBusy = false;
            SelectedProxyStudent = await ProxyStudents.Refresh(_data, SelectedProxyStudent, _onSyncException);
        }
        finally
        {
            IsBusy = false;
        }
    }
    #endregion

    #region selected
    public Command ClearSelectionsCommand { get; }
    private ProxyEntity? _selectedProxyStudent;
    public ProxyEntity? SelectedProxyStudent
    {
        get => _selectedProxyStudent;
        set
        {
            if (!Set(ref _selectedProxyStudent, value)) return;
            ChangingStudent = value?.Name;
            SetInOutCourses();
        }
    }

    private async void SetInOutCourses()
    {
        var buffer = new ProxyCourse[SelectedProxyCourses.Count];
        SelectedProxyCourses.CopyTo(buffer, 0);
        ClearSelections(false);

        var mustThrow = false;
        if (SelectedProxyStudent is not null)
        {
            try
            {
                var student = await _data.StudentsRep
                    .GetStudentByIdAsync(SelectedProxyStudent.Id);
                if (student is not null)
                {
                    var comparer = new SelectedComparer<ProxyCourse>(buffer);

                    var outs = _data.CoursesRep.Items
                        .Where(c => c.Students.Contains(student))
                        .Select(c => new ProxyCourse(c));
                    if (outs.Any()) await ReloadingAsync(outs, InProxyCourses, comparer);

                    var ins = _data.CoursesRep.Items
                        .Where(c => !c.Students.Contains(student))
                        .Select(c => new ProxyCourse(c));
                    if (ins.Any()) await ReloadingAsync(ins, OutProxyCourses, comparer);
                }
                else mustThrow = true;
            }
            catch 
            {
                _warning?.ShowWarning("Для корректной работы дождитесь окончания асинхронных действий.", false);
            }
        }
        foreach (var item in buffer) SelectedProxyCourses.Add(item);
        if (mustThrow)
            throw new WarningException("\"Студент в работе\" был удалён");
        //Thread.Sleep(100);
        RaiseSetUnsetCanExecuteChanged();
    }

    private ProxyCourse? _selectedProxyCourse;
    public ProxyCourse? SelectedProxyCourse
    {
        get => _selectedProxyCourse;
        set
        {
            if (!Set(ref _selectedProxyCourse, value)) return;
            CourseDeleteAsyncCommand.RaiseCanExecuteChanged();
            if (value is null 
                || SelectedProxyCourses.Contains(value) 
                || AddSelectedCourse(value)) 
                return;
            SelectedProxyCourses.Add(value);
            SetInOutCourses();
        }
    }

    private bool AddSelectedCourse(ProxyCourse value)
    {
        if (SelectedProxyStudent is not null) return false;
        InProxyCourses.Add(value);
        OutProxyCourses.Add(value);
        SelectedProxyCourses.Add(value);
        return true;
    }

    private void ClearSelections() => ClearSelections(true);
    private void ClearSelections(bool withProxy)
    {
        if (withProxy)
        {
            SelectedProxyStudent = null;
            SelectedProxyCourse = null;
        }
        SelectedProxyCourses.Clear();
        InProxyCourses.Clear();
        OutProxyCourses.Clear();
    }
    #endregion

    #region weather

    private readonly AsyncCommand _bonusAsyncCommand;
    public AsyncCommand RefreshBonusAsyncCommand { get;}
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
        set => Set(ref _timeString, value);
    }

    private byte[]? _weatherImage = Icons._10d;
    public byte[]? WeatherImage
    {
        get => _weatherImage;
        set => Set(ref _weatherImage, value);
    }

    private string? _weatherString;
    public string? WeatherString
    {
        get => _weatherString;
        set => Set(ref _weatherString, value);
    }

    private string? _weatherToolTipsString;
    public string? WeatherToolTipsString
    {
        get => _weatherToolTipsString;
        set => Set(ref _weatherToolTipsString, value);
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
        private set => Set(ref _buttonText, value);
    }

    private string _buttonToolTip = ShowTip;
    public string ButtonToolTip
    {
        get => _buttonToolTip;
        private set => Set(ref _buttonToolTip, value);
    }

    private void Bonus()
    {
        if (!_isBonusBusy)
        {
            ((ICommand)_bonusAsyncCommand).Execute(null);
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
        RefreshBonusAsyncCommand.ResetCancel(ref _infoCts);
        _bonusAsyncCommand.ResetCancel(ref _infoCts);
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
                    throw new OperationCanceledException(IErrorCancelHandler.NoHandle);
                if (_stopwatch.Elapsed.Minutes < 5 && notFirst) continue;
                notFirst = true;
                (RefreshBonusAsyncCommand as ICommand).Execute(null);
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

    #region IDisposable pattern
    private bool _disposedValue;
    protected virtual void Dispose(bool disposing)
    {
        if (_disposedValue) return;
        if (disposing)
        {
            RefreshBonusAsyncCommand.Cancel();
            _bonusAsyncCommand.Cancel();
            _progressLoadingAsyncCommand.Cancel();
            _infoCts.Dispose();
            _reloadCts?.Dispose();
            _data.Dispose();
        }

        // TODO: освободить неуправляемые ресурсы (неуправляемые объекты) и переопределить метод завершения
        // TODO: установить значение NULL для больших полей
        _disposedValue = true;
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