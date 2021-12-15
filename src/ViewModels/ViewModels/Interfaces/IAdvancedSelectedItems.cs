using Models.Entities.Proxies;

namespace ViewModels.Interfaces;

public interface IAdvancedSelectedItems<T> 
{
    void SelectItems(IList<T> items);
    void SelectItem(T? item = default);
}