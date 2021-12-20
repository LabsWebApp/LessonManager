using System.Collections.ObjectModel;
using Models;
using Models.Entities.Proxies;
using ViewModelBase.Commands;
using ViewModelBase.Commands.ErrorHandlers;

namespace ViewModels.Helpers;

public static class Extensions
{
    public static async Task<TProxy> Refresh<TProxy>(
        this ObservableCollection<TProxy> proxies,
        DataManager data,
        TProxy? proxy, Action<object?> onSyncException) where TProxy : ProxyEntity
    {
        if (proxy is null || !proxies.Any())
            throw new SynchronizationDataException(onSyncException);
        var entity = proxy;
        var refreshItem = proxies.FirstOrDefault(p => p.Id == entity.Id);
        if (refreshItem == default) throw new SynchronizationDataException(onSyncException);

        var (studentsRep, coursesRep, _) = data;
        ProxyEntity newData = refreshItem switch
        {
            ProxyStudent => new ProxyStudent(await studentsRep.GetStudentByIdAsync(proxy.Id) ??
                             throw new SynchronizationDataException(onSyncException)),
            ProxyCourse => new ProxyCourse(await coursesRep.GetCourseByIdAsync(proxy.Id) ??
                            throw new SynchronizationDataException(onSyncException)),
            _ => throw new Exception()
        };

        var ind = proxies.IndexOf(refreshItem);

        if (proxies[ind].Name != newData.Name) 
            proxies[ind] = proxies[ind] with { Name = newData.Name };
        if (proxies[ind].Count != newData.Count) 
            proxies[ind] = proxies[ind] with { Count = newData.Count };

        return proxies[ind];
    }

    public static void ShowWarning(this IWarningHandler handler, string message, bool err = true)
    {
        if (message == IErrorHandler.NoHandle) return;
        if (err)
            handler.HandleError(new Exception(message));
        else handler.WarningHandle(new WarningException(message));
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
