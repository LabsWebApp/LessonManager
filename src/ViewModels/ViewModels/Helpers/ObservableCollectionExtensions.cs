using System.Collections.ObjectModel;
using Models;
using Models.Entities.Proxies;

namespace ViewModels.Helpers;

public static class ObservableCollectionExtensions
{
    /// <summary>
    /// База данных была изменена извне - перегрузите её.
    /// </summary>
    private const string SyncErrMsg = MainViewModel.SyncErrMsg;

    public static void Refresh<TProxy>(
        this ObservableCollection<TProxy>? proxies,
        DataManager data,
        TProxy? proxy) where TProxy : ProxyEntity
    {
        if (proxy is null || proxies is null || !proxies.Any())
            throw new SynchronizationLockException(SyncErrMsg);
        var entity = proxy;
        var refreshItem = proxies.FirstOrDefault(p => p.Id == entity.Id);
        if (refreshItem == default) throw new SynchronizationLockException(SyncErrMsg);

        ProxyEntity newData = refreshItem switch
        {
            ProxyStudent => new ProxyStudent(data.StudentsRep.GetStudentById(proxy.Id) ??
                             throw new SynchronizationLockException(SyncErrMsg)),
            ProxyCourse => new ProxyCourse(data.CoursesRep.GetCourseById(proxy.Id) ??
                            throw new SynchronizationLockException(SyncErrMsg)),
            _ => throw new Exception()
        };

        var ind = proxies.IndexOf(refreshItem);

        if (proxies[ind].Name != newData.Name) 
            proxies[ind] = proxies[ind] with { Name = newData.Name };
        if (proxies[ind].Count != newData.Count) 
            proxies[ind] = proxies[ind] with { Count = newData.Count };
    }

    //public static void SetSelect(this ObservableCollection<ProxyCourse>? courses, Guid id, bool add = true)
    //{
    //    if (courses == null) return;
    //    if (!add)
    //    {
    //        foreach (var course in courses.Where(c => c.IsSelected && c.Id != id)) 
    //        {
    //            var ind = courses.IndexOf(course);
    //            courses[ind] = courses[ind] with { IsSelected = false };
    //        }
    //    }

    //    var mustDo = courses.FirstOrDefault(c => !c.IsSelected && c.Id == id);
    //    if (mustDo != null)
    //    {
    //        var ind = courses.IndexOf(mustDo);
    //        courses[ind] = courses[ind] with { IsSelected = true };
    //    }
    //}

    //public static void SetSelect(
    //    this ObservableCollection<ProxyCourse>? courses, ProxyCourse course, bool add = true) =>
    //    SetSelect(courses, course.Id, add);
}
