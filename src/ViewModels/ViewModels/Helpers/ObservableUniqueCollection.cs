using System.Collections;
using System.Collections.ObjectModel;

namespace ViewModels.Helpers;

public class ObservableUniqueCollection<T> : ObservableCollection<T>
{
    private readonly HashSet<T> _hashSet;

    public ObservableUniqueCollection() : this(EqualityComparer<T>.Default) { }

    public ObservableUniqueCollection(IEqualityComparer<T> equalityComparer) => 
        _hashSet = new HashSet<T>(equalityComparer);

    public void AddRange(IEnumerable<T> items)
    {
        foreach (var item in items)
        {
            InsertItem(Count, item);
        }
    }

    protected override void InsertItem(int index, T item)
    {
        if (_hashSet.Add(item))
        {
            base.InsertItem(index, item);
        }
    }

    protected override void ClearItems()
    {
        base.ClearItems();
        _hashSet.Clear();
    }

    protected override void RemoveItem(int index)
    {
        var item = this[index];
        _hashSet.Remove(item);
        base.RemoveItem(index);
    }

    protected override void SetItem(int index, T item)
    {
        if (_hashSet.Add(item))
        {
            var oldItem = this[index];
            _hashSet.Remove(oldItem);
            base.SetItem(index, item);
        }
    }

    public bool IsIntersect(IEnumerable? objects)
    {
        if (objects is null) return false;
        foreach (var obj in objects)
        {
            if (obj is T t && _hashSet.Contains(t)) return true;
        }
        return false;
    }
}