namespace Models.Entities.Proxies;

public class IdProxyEntityComparer : IEqualityComparer<ProxyEntity>
{
    public bool Equals(ProxyEntity? x, ProxyEntity? y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (x is null) return false;
        if (y is null) return false;
        return x.GetType() == y.GetType() && x.Id.Equals(y.Id);
    }

    public int GetHashCode(ProxyEntity obj) => obj.Id.GetHashCode();
}