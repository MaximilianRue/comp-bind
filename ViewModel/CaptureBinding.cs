using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompBind.ViewModel
{
    public class CaptureBinding<OutputType> :
        BaseBinding,
        IDataBindingOutput<OutputType>,
        IDataBindingOutputReadonly<OutputType>
    {
        protected Action<OutputType> setter;
        protected Func<OutputType> getter;
        protected Dictionary<string, IDataBindingInput<OutputType>> children = new Dictionary<string, IDataBindingInput<OutputType>>();

        public CaptureBinding() { }
        public CaptureBinding(CaptureBinding<OutputType> other)
        {
            setter = other.setter;
            getter = other.getter;
            children = new Dictionary<string, IDataBindingInput<OutputType>>(other.children);
        }

        public OutputType GetValue()
        {
            return getter();
        }

        public void SetValue(OutputType value)
        {
            setter(value);
            NotifyDataBindingChanged();
        }

        public override IDataBinding Step(PathElement element)
        {
            if (element.Type != PathElement.PathElementType.Member)
            {
                throw new ArgumentException("Field based bindings may only step along member typed path elements.");
            }
            return children[element.FieldName];
        }

        public void AddSubBinding(IDataBindingInput<OutputType> subBinding, PathElement pe)
        {
            if (pe.Type != PathElement.PathElementType.Member)
            {
                throw new ArgumentException("Field based bindings may only hold children with member typed path elements.");
            }
            subBinding.LocalPath = pe;
            subBinding.ManagingScope = ManagingScope;

            children[pe.FieldName] = subBinding;
        }

        public void SetSetter(Action<OutputType> setter)
        {
            this.setter = setter;
        }

        public void SetGetter(Func<OutputType> getter)
        {
            this.getter = getter;
        }

        public override IDataBinding Clone()
        {
            return new CaptureBinding<OutputType>(this);
        }
    }
}
