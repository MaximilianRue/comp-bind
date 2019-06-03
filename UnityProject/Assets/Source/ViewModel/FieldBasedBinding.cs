using System;
using System.Collections.Generic;

namespace CompBind.ViewModel
{
    public class FieldBasedBinding<InputType, OutputType> : BindingNode<InputType, OutputType>
    {
        protected Dictionary<string, IDataBindingInput<OutputType>> children = new Dictionary<string, IDataBindingInput<OutputType>>();

        public FieldBasedBinding(IDataBinding parent) : base(parent) { }
        public FieldBasedBinding(InputType initalValue) : base(initalValue) { }
        public FieldBasedBinding(FieldBasedBinding<InputType, OutputType> other) : base(other)
        {
            children = new Dictionary<string, IDataBindingInput<OutputType>>(other.children);
            foreach (IDataBinding child in children.Values)
            {
                child.Parent = this;
            }
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

        public override IDataBinding Clone()
        {
            return new FieldBasedBinding<InputType, OutputType>(this);
        }

        public void SetSetter(ActionRef<InputType, OutputType> setter)
        {
            valueSetter = setter;
        }
        public void SetGetter(Func<InputType, OutputType> getter)
        {
            valueGetter = getter;
        }
    }
}