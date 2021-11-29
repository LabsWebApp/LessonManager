using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViewModels.Interfaces
{
    public interface IAdvancedSelectedItems : IRefreshable
    {
        void SelectItems(IEnumerable<object> items);
        void SelectItem(object? item = null);
    }
}
