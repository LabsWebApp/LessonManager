using System.Windows.Controls;
using ViewModels.Interfaces;

namespace WpfApp.Controls;

/// <summary>
/// Логика взаимодействия для UserControl1.xaml
/// </summary>
public class MainDataGrid : DataGrid, IRefreshable
{
    public MainDataGrid()
    {
        SelectionMode = DataGridSelectionMode.Single;
        AutoGenerateColumns = false;
        IsReadOnly = true;
    }
    public void Refresh() => Items.Refresh();
}