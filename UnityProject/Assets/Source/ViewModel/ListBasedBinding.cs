using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompBind.ViewModel
{
    public class ListBasedBinding<ListEntryType, ListEntryOutputType> :
        BindingNode<List<ListEntryType>, List<ListEntryOutputType>>,
        IChildDependendOutputBinding<ListEntryOutputType>
    {
        protected Action<ListEntryType, ListEntryOutputType> entryValueSetter;
        protected Func<ListEntryType, ListEntryOutputType> entryValueGetter;
        protected IDataBindingInput<ListEntryOutputType> entryBindingTemplate;

        public ListBasedBinding(List<ListEntryType> initialValue) : base(initialValue)
        {
            initialize();
        }

        public ListBasedBinding(IDataBinding parent) : base(parent)
        {
            initialize();
        }

        public ListBasedBinding(ListBasedBinding<ListEntryType, ListEntryOutputType> other) : base(other)
        {
            valueGetter = other.valueGetter;
            valueSetter = other.valueSetter;
            entryBindingTemplate = other.entryBindingTemplate.Clone() as IDataBindingInput<ListEntryOutputType>;
        }

        private void initialize()
        {
            // By default, entries are handled by a simple field based binding
            var defaultTemplate = new FieldBasedBinding<ListEntryOutputType, ListEntryOutputType>(this);
            defaultTemplate.SetGetter((value) => value);
            defaultTemplate.SetSetter((ref ListEntryOutputType current, ListEntryOutputType newValue) => current = newValue);
            entryBindingTemplate = defaultTemplate;

            // Capsule entry value properties
            valueGetter = (boundList) =>
            {
                List<ListEntryOutputType> list = new List<ListEntryOutputType>(boundList.Count);
                for (int i = 0; i < boundList.Count; i++)
                {
                    list.Add(entryValueGetter(boundList[i]));
                }
                return list;
            };
            valueSetter = (ref List<ListEntryType> boundList, List<ListEntryOutputType> newValueList) =>
            {
                boundList.Clear();
                foreach (ListEntryOutputType entry in newValueList)
                {
                    ListEntryType updatedEntry = default;
                    entryValueSetter(updatedEntry, entry);
                    boundList.Add(updatedEntry);
                }
            };
        }

        public override IDataBinding Step(PathElement element)
        {
            if (element.Type != PathElement.PathElementType.Index)
            {
                throw new ArgumentException("List based bindings can only step along index-type path elements.");
            }
            return BindingAt(element.Index);
        }

        public ListEntryOutputType GetChildDependendOutput(PathElement childPathElement)
        {
            if (childPathElement.Type != PathElement.PathElementType.Index)
            {
                throw new ArgumentException("List based bindings can only step along index-type path elements.");
            }
            updateCapsuledObject();
            return entryValueGetter(capsuledObject[childPathElement.Index]);
        }

        public override IDataBinding Clone()
        {
            return new ListBasedBinding<ListEntryType, ListEntryOutputType>(this);
        }

        #region Setup helpers

        public EntryNode BindEntryTemplate<EntryNode>(EntryNode objectBinding)
            where EntryNode : IDataBindingInput<ListEntryOutputType>
        {
            entryBindingTemplate = objectBinding;
            return (EntryNode)entryBindingTemplate;
        }
        public FieldBasedBinding<ListEntryOutputType, ListEntryOutputType> BindEntryTemplate()
        {
            entryBindingTemplate = new FieldBasedBinding<ListEntryOutputType, ListEntryOutputType>(this);
            return entryBindingTemplate as FieldBasedBinding<ListEntryOutputType, ListEntryOutputType>;
        }
        public FieldBasedBinding<ListEntryOutputType, EntryTemplateOutputType> BindEntryTemplate<EntryTemplateOutputType>()
        {
            entryBindingTemplate = new FieldBasedBinding<ListEntryOutputType, EntryTemplateOutputType>(this);
            return entryBindingTemplate as FieldBasedBinding<ListEntryOutputType, EntryTemplateOutputType>;
        }

        #endregion

        #region List Functionality

        public IDataBindingInput<ListEntryOutputType> this[int i]
        {
            get { return BindingAt(i); }
        }

        public IDataBindingInput<ListEntryOutputType> BindingAt(int index)
        {
            IDataBindingInput<ListEntryOutputType> newBinding = (IDataBindingInput<ListEntryOutputType>)entryBindingTemplate.Clone();
            PathElement pe = new PathElement(index);
            newBinding.LocalPath = pe;
            newBinding.Parent = this;
            return newBinding;
        }

        public void SetEntrySetter(Action<ListEntryType, ListEntryOutputType> setter)
        {
            entryValueSetter = setter;
        }

        public void SetEntryGetter(Func<ListEntryType, ListEntryOutputType> getter)
        {
            entryValueGetter = getter;
        }

        #endregion
    }
}
