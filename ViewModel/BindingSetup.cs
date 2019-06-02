using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompBind.ViewModel
{
    public abstract class BindingSetup
    {
        protected Scope scope;
        private IDataBinding managedBinding;

        public BindingSetup(IDataBinding db)
        {
            scope = db.ManagingScope;
        }
    }

    public class CaptureBindingSetup<OutputType> : BindingSetup
    {
        public CaptureBinding<OutputType> ManagedBinding { get; protected set; }

        public CaptureBindingSetup(CaptureBinding<OutputType> managedBinding) : base(managedBinding)
        {
            ManagedBinding = managedBinding;
        }

        public CaptureBindingSetup<OutputType> SetSetter(Action<OutputType> setter)
        {
            ManagedBinding.SetSetter(setter);
            return this;
        }

        public CaptureBindingSetup<OutputType> SetGetter(Func<OutputType> getter)
        {
            ManagedBinding.SetGetter(getter);
            return this;
        }

        public FieldBasedBindingSetup<OutputType, MemberOutputType> BindMember<MemberOutputType>(string fieldName)
        {
            PathElement pe = new PathElement(fieldName);
            FieldBasedBinding<OutputType, MemberOutputType> memberBinding = new FieldBasedBinding<OutputType, MemberOutputType>(ManagedBinding);
            ManagedBinding.AddSubBinding(memberBinding, pe);

            // Start new setup for member object
            FieldBasedBindingSetup<OutputType, MemberOutputType> memberSetup = new FieldBasedBindingSetup<OutputType, MemberOutputType>(memberBinding);
            return memberSetup;
        }

        public CaptureBindingSetup<OutputType> LinkToPath(Path parentingPath)
        {
            scope.LinkPaths(parentingPath, ManagedBinding.GetAbsoluteBindingPath());
            return this;
        }
        public CaptureBindingSetup<OutputType> LinkDependendPath(Path dependendPath)
        {
            scope.LinkPaths(ManagedBinding.GetAbsoluteBindingPath(), dependendPath);
            return this;
        }
    }

    public class FieldBasedBindingSetup<InputType, OutputType> : BindingSetup
    {
        public FieldBasedBinding<InputType, OutputType> ManagedBinding { get; protected set; }

        public FieldBasedBindingSetup(FieldBasedBinding<InputType, OutputType> managedBinding) : base(managedBinding)
        {
            this.ManagedBinding = managedBinding;
        }

        public FieldBasedBindingSetup<InputType, OutputType> SetSetter(ActionRef<InputType, OutputType> setter)
        {
            ManagedBinding.SetSetter(setter);
            return this;
        }
        public FieldBasedBindingSetup<InputType, OutputType> SetGetter(Func<InputType, OutputType> getter)
        {
            ManagedBinding.SetGetter(getter);
            return this;
        }
        public FieldBasedBindingSetup<OutputType, MemberOutputType> BindMember<MemberOutputType>(string fieldName)
        {
            PathElement pe = new PathElement(fieldName);
            FieldBasedBinding<OutputType, MemberOutputType> memberBinding = new FieldBasedBinding<OutputType, MemberOutputType>(ManagedBinding);
            ManagedBinding.AddSubBinding(memberBinding, pe);

            // Start new setup for member object
            FieldBasedBindingSetup<OutputType, MemberOutputType> memberSetup = new FieldBasedBindingSetup<OutputType, MemberOutputType>(memberBinding);
            return memberSetup;
        }
        public FieldBasedBindingSetup<InputType, OutputType> LinkToPath(Path parentingPath)
        {
            scope.LinkPaths(parentingPath, ManagedBinding.GetAbsoluteBindingPath());
            return this;
        }
        public FieldBasedBindingSetup<InputType, OutputType> LinkDependendPath(Path dependendPath)
        {
            scope.LinkPaths(ManagedBinding.GetAbsoluteBindingPath(), dependendPath);
            return this;
        }
    }

    public class ListBasedBindingSetup<ListEntryType, ListEntryOutputType> : BindingSetup
    {
        public ListBasedBinding<ListEntryType, ListEntryOutputType> ManagedBinding { get; protected set; }

        public ListBasedBindingSetup(ListBasedBinding<ListEntryType, ListEntryOutputType> listBinding) : base(listBinding)
        {
            ManagedBinding = listBinding;
        }

        public FieldBasedBindingSetup<ListEntryOutputType, ListEntryOutputType> BindEntryTemplate()
        {
            var template = ManagedBinding.BindEntryTemplate();

            // Make default get / set
            template.SetGetter((val) => val);
            template.SetSetter((ref ListEntryOutputType val, ListEntryOutputType newVal) => val = newVal);

            return new FieldBasedBindingSetup<ListEntryOutputType, ListEntryOutputType>(template);
        }

        public FieldBasedBindingSetup<ListEntryOutputType, EntryTemplateOutputType> BindEntryTemplate<EntryTemplateOutputType>()
        {
            var template = ManagedBinding.BindEntryTemplate<EntryTemplateOutputType>();
            return new FieldBasedBindingSetup<ListEntryOutputType, EntryTemplateOutputType>(template);
        }

        public ListBasedBindingSetup<ListEntryType, ListEntryOutputType> SetEntrySetter(Action<ListEntryType, ListEntryOutputType> setter)
        {
            ManagedBinding.SetEntrySetter(setter);
            return this;
        }

        public ListBasedBindingSetup<ListEntryType, ListEntryOutputType> SetEntryGetter(Func<ListEntryType, ListEntryOutputType> getter)
        {
            ManagedBinding.SetEntryGetter(getter);
            return this;
        }

        public ListBasedBindingSetup<ListEntryType, ListEntryOutputType> LinkToPath(Path parentingPath)
        {
            scope.LinkPaths(parentingPath, ManagedBinding.GetAbsoluteBindingPath());
            return this;
        }
        public ListBasedBindingSetup<ListEntryType, ListEntryOutputType> LinkDependendPath(Path dependendPath)
        {
            scope.LinkPaths(ManagedBinding.GetAbsoluteBindingPath(), dependendPath);
            return this;
        }
    }
}