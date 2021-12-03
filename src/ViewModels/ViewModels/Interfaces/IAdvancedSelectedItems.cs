namespace ViewModels.Interfaces
{
    public interface IAdvancedSelectedItems : IRefreshable
    {
        void SelectItems(IEnumerable<object> items);
        void SelectItem(object? item = null);
    }
}
