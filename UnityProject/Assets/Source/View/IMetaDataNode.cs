using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CompBind.ViewModel;

namespace CompBind.View
{
    /// <summary>
    /// Acts as data source for child value update receivers, but
    /// also receives value updates itself.
    /// </summary>
    /// <remarks>
    /// Useful for implementing notes like lists, that have to
    /// handle the creation and data access of childs dynamically.
    /// </remarks>
    public interface IMetaDataNode : IValueUpdateReceiver, IDataNode { }
}