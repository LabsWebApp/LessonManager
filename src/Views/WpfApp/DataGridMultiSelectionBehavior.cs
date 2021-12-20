using System.Collections;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Xaml.Behaviors;

namespace WpfApp;

public class DataGridMultiSelectionBehavior : Behavior<DataGrid>
{
    public static readonly DependencyProperty SelectedItemsProperty =
        DependencyProperty.Register("SelectedItems", typeof(IList), typeof(DataGridMultiSelectionBehavior),
            new UIPropertyMetadata(null, SelectedItemsChanged));

    public IList? SelectedItems
    {
        get => (IList)GetValue(SelectedItemsProperty);
        set => SetValue(SelectedItemsProperty, value);
    }

    private bool _isUpdatingTarget;
    private bool _isUpdatingSource;

    private static void SelectedItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not DataGridMultiSelectionBehavior behavior) return;

        var newValue = e.NewValue as INotifyCollectionChanged;

        if (e.OldValue is INotifyCollectionChanged oldValue)
        {
            oldValue.CollectionChanged -= behavior.SourceCollectionChanged!;
            behavior.AssociatedObject.SelectionChanged -= behavior.DataGridSelectionChanged;
        }

        if (newValue != null)
        {
            behavior.AssociatedObject.SelectedItems.Clear();
            foreach (object item in (IEnumerable)newValue)
            {
                behavior.AssociatedObject.SelectedItems.Add(item);
            }
        }

        behavior.AssociatedObject.SelectionChanged += behavior.DataGridSelectionChanged;
        if (newValue != null) newValue.CollectionChanged += behavior.SourceCollectionChanged!;
    }
    private void DataGridSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_isUpdatingTarget) return;

        var selectedItems = SelectedItems;

        if (selectedItems is null) return;

        try
        {
            _isUpdatingSource = true;

            foreach (object item in e.RemovedItems)
            {
                selectedItems.Remove(item);
            }

            foreach (var item in e.AddedItems)
            {
                selectedItems.Add(item);
            }
        }
        finally
        {
            _isUpdatingSource = false;
        }
    }
    private void SourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        if (_isUpdatingSource) return;

        try
        {
            _isUpdatingTarget = true;

            if (e.OldItems != null)
            {
                foreach (var item in e.OldItems)
                {
                    AssociatedObject.SelectedItems.Remove(item);
                }
            }

            if (e.NewItems != null)
            {
                foreach (var item in e.NewItems)
                {
                    AssociatedObject.SelectedItems.Add(item);
                }
            }

            if (e.Action == NotifyCollectionChangedAction.Reset)
                AssociatedObject.SelectedItems.Clear();
        }
        finally
        {
            _isUpdatingTarget = false;
        }
    }
    protected override void OnAttached()
    {
        base.OnAttached();
        if (SelectedItems == null) return;

        AssociatedObject.SelectedItems.Clear();
        foreach (var item in SelectedItems)
        {
            AssociatedObject.SelectedItems.Add(item);
        }
    }
}