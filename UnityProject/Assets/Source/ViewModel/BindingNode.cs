using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompBind.ViewModel
{
    /// <summary>
    /// Binding that acts upon a capsuled object. Offers set and get functionality.
    /// </summary>
    /// <typeparam name="InputType">Type of the bound object.</typeparam>
    /// <typeparam name="OutputType">Type that is used for getting / setting.</typeparam>
    public abstract class BindingNode<InputType, OutputType> : 
        BaseBinding, 
        IDataBindingInput<InputType>, 
        IDataBindingOutput<OutputType>,
        IDataBindingOutputReadonly<OutputType>
    {
        protected InputType capsuledObject;
        protected ActionRef<InputType, OutputType> valueSetter;
        protected Func<InputType, OutputType> valueGetter;

        public BindingNode(IDataBinding parent)
        {
            Parent = parent;
        }
        public BindingNode(InputType initialValue)
        {
            capsuledObject = initialValue;
        }
        public BindingNode(BindingNode<InputType, OutputType> other)
        {
            valueGetter = other.valueGetter;
            valueSetter = other.valueSetter;
            ManagingScope = other.ManagingScope;
            capsuledObject = other.capsuledObject;
        }
                
        public virtual OutputType GetValue()
        {
            if(valueGetter == null)
            {
                throw new InvalidOperationException("Can't get value! No getter was set for: '" + this.GetAbsoluteBindingPath() + "'");
            }

            updateCapsuledObject();
            return valueGetter(capsuledObject);
        }

        public virtual void SetValue(OutputType value)
        {
            if (valueSetter == null)
            {
                throw new InvalidOperationException("Binding is readonly! No setter was set for: '" + this.GetAbsoluteBindingPath() + "'");
            }

            updateCapsuledObject();
            valueSetter(ref capsuledObject, value);
            NotifyDataBindingChanged();
        }

        protected virtual void updateCapsuledObject()
        {
            if (Parent != null)
            {
                if(Parent is IChildDependendOutputBinding<InputType>)
                {
                    capsuledObject = (Parent as IChildDependendOutputBinding<InputType>).GetChildDependendOutput(LocalPath);
                }
                else
                {
                    capsuledObject = ((IDataBindingOutputReadonly<InputType>)Parent).GetValue();
                }
            }
        }
    }    
}
