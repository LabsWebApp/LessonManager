using System.Collections.Generic;
using System.Windows.Controls;
using ViewModels.Interfaces;

namespace WpfApp.Controls
{
    public class AdvancedDataGrid : DataGrid, IAdvancedSelectedItems
    {
        public AdvancedDataGrid()
        {
            SelectionMode = DataGridSelectionMode.Extended;
            AutoGenerateColumns = false;
            IsReadOnly = true;
        }
        public void Refresh() => Items.Refresh();
        public void SelectItems(IEnumerable<object> items)
        {
            SelectedIndex = -1;
            foreach (var item in items)
            {
                SelectedItems.Add(item);
            }
        }

        public void SelectItem(object? item = null)
        {
            if (item is null)
            {
                SelectedIndex = -1;
                return;
            }
            if(!Items.Contains(item) || SelectedItems.Contains(item)) return;
            SelectedItems.Add(item);
        }
    }
}
