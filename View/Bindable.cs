using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CompBind.ViewModel;
using UnityEngine;

namespace CompBind.View
{
    class Bindable<ReceivingType> : IValueUpdateReceiver
    {
        public Action<ReceivingType> UpdateAction { get; set; }

        public Path BindingPath { get; set; }
        public IDataNode Context { get; set; }

        public void ForceUpdate()
        {
            IDataBindingOutputReadonly<ReceivingType> db = Context.GetBinding(BindingPath) as IDataBindingOutputReadonly<ReceivingType>;
            UpdateAction(db.GetValue());
        }

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