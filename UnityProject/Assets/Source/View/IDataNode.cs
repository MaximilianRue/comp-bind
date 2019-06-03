using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CompBind;
using CompBind.ViewModel;

namespace CompBind.View
{
    /// <summary>
    /// Acts as a data source and registration point for update receivers.
    /// </summary>
    public interface IDataNode
    {
        /// <summary>
        /// Acts as initialization point for the datanode.
        /// </summary>
        void InitializeDataNode();

        /// <summary>
        /// Registers value update receivers to this datanode.
        /// </summary>
        /// <remarks>
        /// Datanode will trigger receiver updates.
        /// </remarks>
        /// <param name="updateReceivers">
        /// Receivers that should be newly added.
        /// </param>
        void RegisterUpdateReceivers(List<IValueUpdateReceiver> updateReceivers);

        /// <summary>
        /// Returns the databinding object behind the passed path.
        /// </summary>
        /// <remarks>
        /// Allows 
        /// </remarks>
        /// <param name="path"></param>
        /// <returns></returns>
        IDataBinding GetBinding(Path path);
    }
}