using Models.Entities.Proxies;

namespace ViewModels.Helpers;

internal class SelectedComparer<TProxyEntity> : IComparer<Guid> 
    where TProxyEntity : ProxyEntity
{
    private readonly IList<TProxyEntity> _entities;

    public SelectedComparer(IList<TProxyEntity> entities) =>
        _entities = entities;


    public int Compare(Guid x, Guid y)
    {
        var hasX = _entities.Any(e => e.Id == x);
        var hasY = _entities.Any(e => e.Id == y);
        return hasX switch
        {
            true when !hasY => -1,
            false when hasY => 1,
            _ => 0
        };
    }
}