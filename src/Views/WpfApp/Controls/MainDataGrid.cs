using System.Windows.Controls;

namespace WpfApp.Controls;

/// <summary>
/// Логика взаимодействия для UserControl1.xaml
/// </summary>
public class MainDataGrid : DataGrid
{
    public MainDataGrid()
    {
        SelectionMode = DataGridSelectionMode.Single;
        AutoGenerateColumns = false;
        IsReadOnly = true;
    }
}