using System.Windows.Controls;
using ViewModels;

namespace WpfApp.Controls;

/// <summary>
/// Логика взаимодействия для UserControl1.xaml
/// </summary>
public class MainDataGrid : DataGrid, IRefreshable
{
    public MainDataGrid()
    {
        SelectionMode = System.Windows.Controls.DataGridSelectionMode.Single;
        AutoGenerateColumns = false;
        IsReadOnly = true;
    }
    public void Refresh() => Items.Refresh();
}