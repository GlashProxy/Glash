using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Glash.Blazor.Client
{
    public interface INavigator
    {
        void Alert(string title, string message);
        void Navigate<TPage>(Dictionary<string, object> parameterDict = null);
    }
}
