using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompBind.View
{
    /// <summary>
    /// Acts as source for value update receivers, as
    /// one component may return multiple ones.
    /// </summary>
    public interface IBindableComponent
    {
        /// <summary>
        /// Initializes bindable component, e.g. to initialize the
        /// value update receivers.
        /// </summary>
        void InitializeBindableComponent();

        /// <summary>
        /// Returns list of all value update receivers.
        /// </summary>
        /// <returns></returns>
        List<IValueUpdateReceiver> GetValueUpdateReceivers();
    }
}
