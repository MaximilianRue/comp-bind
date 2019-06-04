using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CompBind.ViewModel;
using UnityEngine;

namespace CompBind.View
{
    /// <summary>
    /// Default implementation for a simple update receiver.
    /// </summary>
    /// <typeparam name="ReceivingType">Type of received value for the update function.</typeparam>
    class Bindable<ReceivingType> : IValueUpdateReceiver
    {
        /// <summary>
        /// Action that will be triggered in case of an update.
        /// </summary>
        public Action<ReceivingType> UpdateAction { get; set; }

        /// <summary>
        /// Path this UpdateReceiver is listening for.
        /// </summary>
        public Path BindingPath { get; set; }

        /// <summary>
        /// DataNode this UpdateReceiver uses for accessing the viewmodel.
        /// </summary>
        public IDataNode Context { get; set; }

        /// <summary>
        /// Will trigger an update. Will pull value from the viewmodel and trigger the update
        /// action.
        /// </summary>
        public void ForceUpdate()
        {
            try
            {
                IDataBindingOutputReadonly<ReceivingType> db = 
                    Context.GetBinding(BindingPath) as IDataBindingOutputReadonly<ReceivingType>;
                UpdateAction(db.GetValue());
            }
            catch (ArgumentException)
            {
                Debug.LogWarning("Binding at '" + this.GetAbsoluteDataPath() + "' not found.");
            }
        }

        /// <summary>
        /// Is called by the managing DataNode when an update is broadcasted.
        /// </summary>
        /// <param name="updatedPath">Path that received a value update.</param>
        public void OnUpdate(Path updatedPath)
        {
            // Check if this does influence this binding
            if (BindingPath.IsSamePath(updatedPath) || BindingPath.IsSubpathOf(updatedPath))
            {
                ForceUpdate();
            }
        }
    }
}