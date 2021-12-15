using System;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;

namespace WpfApp;

public class RowToIndexConvertor : MarkupExtension, IValueConverter
{
    private static RowToIndexConvertor? _convertor;

    public object Convert(
        object? value,
        Type targetType,
        object? parameter,
        System.Globalization.CultureInfo culture) =>
        value is DataGridRow row ? row.GetIndex() + 1 : -1;

    public override object ProvideValue(IServiceProvider serviceProvider) =>
        _convertor ??= new RowToIndexConvertor();

    public object ConvertBack(object? value, 
        Type targetType, 
        object? parameter, 
        System.Globalization.CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}