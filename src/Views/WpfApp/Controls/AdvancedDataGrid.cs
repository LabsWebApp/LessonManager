using System.Collections.Generic;
using System.Windows.Controls;
using Models.Entities.Proxies;
using ViewModels.Interfaces;

namespace WpfApp.Controls;

public class AdvancedDataGrid : DataGrid, IAdvancedSelectedItems<ProxyCourse>
{
    public AdvancedDataGrid()
    {
        SelectionMode = DataGridSelectionMode.Extended;
        AutoGenerateColumns = false;
        IsReadOnly = true;
    }

    public void SelectItems(IList<ProxyCourse> items)
    {
        SelectedIndex = -1;
        foreach (var item in items)
        {
            SelectItem(item);
        }
    }

    public void SelectItem(ProxyCourse? item = default)
    {
        if (item is null)
        {
            SelectedIndex = -1;
            return;
        }
        if (Items.Contains(item)) SelectedItems.Add(item);
    }
}