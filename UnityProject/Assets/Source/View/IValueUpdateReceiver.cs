using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompBind.View
{
    /// <summary>
    /// Interface that receives updates to data paths.
    /// </summary>
    public interface IValueUpdateReceiver
    {
        /// <summary>
        /// Represents data path that should trigger a value update action.
        /// </summary>
        Path BindingPath { get; set; }

        /// <summary>
        /// Grants access to the data that is used for updating.
        /// </summary>
        /// <remarks>
        /// May be a local scope, e.g. when using a list binding.
        /// </remarks>
        IDataNode Context { get; set; }

        /// <summary>
        /// Is called when a value update happens.
        /// </summary>
        /// <param name="updatedPath">Path whose value was updated.</param>
        void OnUpdate(Path updatedPath);

        /// <summary>
        /// Triggers the update action of this element.
        /// </summary>
        void ForceUpdate();
    }
}