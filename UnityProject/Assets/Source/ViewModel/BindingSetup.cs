using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompBind.ViewModel
{
    /// <summary>
    /// Manages the creation of a binding. There setup classes
    /// for each available binding type.
    /// </summary>
    public abstract class BindingSetup
    {
        protected Scope scope;
        private IDataBinding managedBinding;

        public BindingSetup(IDataBinding db)
        {
            scope = db.ManagingScope;
        }
    }

    /// <summary>
    /// Manages the setup of a capture binding.
    /// </summary>
    /// <typeparam name="OutputType">Output type of the managed binding.</typeparam>
    public class CaptureBindingSetup<OutputType> : BindingSetup
    {
        public CaptureBinding<OutputType> ManagedBinding { get; protected set; }

        public CaptureBindingSetup(CaptureBinding<OutputType> managedBinding) : base(managedBinding)
        {
            ManagedBinding = managedBinding;
        }

        /// <summary>
        /// Sets the setter for the binding.
        /// </summary>
        /// <param name="setter"></param>
        /// <returns></returns>
        public CaptureBindingSetup<OutputType> SetSetter(Action<OutputType> setter)
        {
            ManagedBinding.SetSetter(setter);
            return this;
        }

        /// <summary>
        /// Sets the getter for the binding.
        /// </summary>
        /// <param name="getter"></param>
        /// <returns></returns>
        public CaptureBindingSetup<OutputType> SetGetter(Func<OutputType> getter)
        {
            ManagedBinding.SetGetter(getter);
            return this;
        }

        /// <summary>
        /// Creates a subbinding to the currently  managed one.
        /// </summary>
        /// <typeparam name="MemberOutputType">Type that is returned by the subbinding.</typeparam>
        /// <param name="fieldName">Path of the subbinding.</param>
        /// <returns></returns>
        public FieldBasedBindingSetup<OutputType, MemberOutputType> BindMember<MemberOutputType>(string fieldName)
        {
            PathElement pe = new PathElement(fieldName);
            FieldBasedBinding<OutputType, MemberOutputType> memberBinding = new FieldBasedBinding<OutputType, MemberOutputType>(ManagedBinding);
            ManagedBinding.AddSubBinding(memberBinding, pe);

            // Start new setup for member object
            FieldBasedBindingSetup<OutputType, MemberOutputType> memberSetup = new FieldBasedBindingSetup<OutputType, MemberOutputType>(memberBinding);
            return memberSetup;
        }

        /// <summary>
        /// Links the managed binding to another path.
        /// </summary>
        /// <param name="parentingPath"></param>
        /// <returns></returns>
        public CaptureBindingSetup<OutputType> LinkToPath(Path parentingPath)
        {
            scope.LinkPaths(parentingPath, ManagedBinding.GetAbsoluteBindingPath());
            return this;
        }

        /// <summary>
        /// Links a path to the managed binding.
        /// </summary>
        /// <param name="dependendPath"></param>
        /// <returns></returns>
        public CaptureBindingSetup<OutputType> LinkDependendPath(Path dependendPath)
        {
            scope.LinkPaths(ManagedBinding.GetAbsoluteBindingPath(), dependendPath);
            return this;
        }
    }

    /// <summary>
    /// Manages the setup of a field based binding.
    /// </summary>
    /// <typeparam name="InputType">Input type of the managed binding.</typeparam>
    /// <typeparam name="OutputType">Output type of the managed binding.</typeparam>
    public class FieldBasedBindingSetup<InputType, OutputType> : BindingSetup
    {
        public FieldBasedBinding<InputType, OutputType> ManagedBinding { get; protected set; }

        public FieldBasedBindingSetup(FieldBasedBinding<InputType, OutputType> managedBinding) : base(managedBinding)
        {
            this.ManagedBinding = managedBinding;
        }

        /// <summary>
        /// Sets the setter for the binding.
        /// </summary>
        /// <param name="setter"></param>
        /// <returns></returns>
        public FieldBasedBindingSetup<InputType, OutputType> SetSetter(ActionRef<InputType, OutputType> setter)
        {
            ManagedBinding.SetSetter(setter);
            return this;
        }

        /// <summary>
        /// Sets the getter for the binding.
        /// </summary>
        /// <param name="setter"></param>
        /// <returns></returns>
        public FieldBasedBindingSetup<InputType, OutputType> SetGetter(Func<InputType, OutputType> getter)
        {
            ManagedBinding.SetGetter(getter);
            return this;
        }

        /// <summary>
        /// Creates a subbinding to the currently  managed one.
        /// </summary>
        /// <typeparam name="MemberOutputType">Type that is returned by the subbinding.</typeparam>
        /// <param name="fieldName">Path of the subbinding.</param>
        /// <returns></returns>
        public FieldBasedBindingSetup<OutputType, MemberOutputType> BindMember<MemberOutputType>(string fieldName)
        {
            PathElement pe = new PathElement(fieldName);
            FieldBasedBinding<OutputType, MemberOutputType> memberBinding = new FieldBasedBinding<OutputType, MemberOutputType>(ManagedBinding);
            ManagedBinding.AddSubBinding(memberBinding, pe);

            // Start new setup for member object
            FieldBasedBindingSetup<OutputType, MemberOutputType> memberSetup = new FieldBasedBindingSetup<OutputType, MemberOutputType>(memberBinding);
            return memberSetup;
        }

        /// <summary>
        /// Links the managed binding to another path.
        /// </summary>
        /// <param name="parentingPath"></param>
        /// <returns></returns>
        public FieldBasedBindingSetup<InputType, OutputType> LinkToPath(Path parentingPath)
        {
            scope.LinkPaths(parentingPath, ManagedBinding.GetAbsoluteBindingPath());
            return this;
        }

        /// <summary>
        /// Links a path to the managed binding.
        /// </summary>
        /// <param name="dependendPath"></param>
        /// <returns></returns>
        public FieldBasedBindingSetup<InputType, OutputType> LinkDependendPath(Path dependendPath)
        {
            scope.LinkPaths(ManagedBinding.GetAbsoluteBindingPath(), dependendPath);
            return this;
        }
    }

    /// <summary>
    /// Manages the setup of a list binding.
    /// </summary>
    /// <typeparam name="ListEntryType">Type of the listentries of the input list.</typeparam>
    /// <typeparam name="ListEntryOutputType">Type of the listentries returned of this binding.</typeparam>
    public class ListBasedBindingSetup<ListEntryType, ListEntryOutputType> : BindingSetup
    {
        public ListBasedBinding<ListEntryType, ListEntryOutputType> ManagedBinding { get; protected set; }

        public ListBasedBindingSetup(ListBasedBinding<ListEntryType, ListEntryOutputType> listBinding) : base(listBinding)
        {
            ManagedBinding = listBinding;
        }

        /// <summary>
        /// Creates a default, field based binding for managing the list entries.
        /// Will make default getters & setters.
        /// </summary>
        /// <returns>Setup of the entry template's binding.</returns>
        public FieldBasedBindingSetup<ListEntryOutputType, ListEntryOutputType> BindEntryTemplate()
        {
            var template = ManagedBinding.BindEntryTemplate();

            // Make default get / set
            template.SetGetter((val) => val);
            template.SetSetter((ref ListEntryOutputType val, ListEntryOutputType newVal) => val = newVal);

            return new FieldBasedBindingSetup<ListEntryOutputType, ListEntryOutputType>(template);
        }

        /// <summary>
        /// Creates a field based binding for managing the list entires.
        /// </summary>
        /// <typeparam name="EntryTemplateOutputType">Output type of the entry binding.</typeparam>
        /// <returns></returns>
        public FieldBasedBindingSetup<ListEntryOutputType, EntryTemplateOutputType> BindEntryTemplate<EntryTemplateOutputType>()
        {
            var template = ManagedBinding.BindEntryTemplate<EntryTemplateOutputType>();
            return new FieldBasedBindingSetup<ListEntryOutputType, EntryTemplateOutputType>(template);
        }

        /// <summary>
        /// Sets setter for a list entry value.
        /// </summary>
        /// <param name="setter"></param>
        /// <returns></returns>
        public ListBasedBindingSetup<ListEntryType, ListEntryOutputType> SetEntrySetter(Action<ListEntryType, ListEntryOutputType> setter)
        {
            ManagedBinding.SetEntrySetter(setter);
            return this;
        }

        /// <summary>
        /// Sets getter for a list entry value.
        /// </summary>
        /// <param name="getter"></param>
        /// <returns></returns>
        public ListBasedBindingSetup<ListEntryType, ListEntryOutputType> SetEntryGetter(Func<ListEntryType, ListEntryOutputType> getter)
        {
            ManagedBinding.SetEntryGetter(getter);
            return this;
        }

        /// <summary>
        /// Links the managed binding to another path.
        /// </summary>
        /// <param name="parentingPath"></param>
        /// <returns></returns>
        public ListBasedBindingSetup<ListEntryType, ListEntryOutputType> LinkToPath(Path parentingPath)
        {
            scope.LinkPaths(parentingPath, ManagedBinding.GetAbsoluteBindingPath());
            return this;
        }

        /// <summary>
        /// Links a path to this binding.
        /// </summary>
        /// <param name="dependendPath"></param>
        /// <returns></returns>
        public ListBasedBindingSetup<ListEntryType, ListEntryOutputType> LinkDependendPath(Path dependendPath)
        {
            scope.LinkPaths(ManagedBinding.GetAbsoluteBindingPath(), dependendPath);
            return this;
        }
    }
}