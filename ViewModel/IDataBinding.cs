using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CompBind;

namespace CompBind.ViewModel
{
    public enum BindingType
    {
        Unspecified,
        Read,
        ReadWrite
    }

    /// <summary>
    /// Base datacontext interface.
    /// </summary>
    /// <remarks>
    /// Allows for navigation between contexts. Basic casting functionality.
    /// </remarks>
    public interface IDataBinding
    {
        IDataBinding Parent { get; set; }
        IDataBinding Step(PathElement element);
        PathElement LocalPath { get; set; }
        Scope ManagingScope { get; set; }
        BindingType BindingType { get; }

        void NotifyDataBindingChanged();

        IDataBinding Clone();

        // For extension
        //T As<T>() where T : class;
        //IDataBinding<InputType, OutputType> WithInputOutputType<InputType, OutputType>();
        //IDataBindingOutput<OutputType> WithOutputType<OutputType>();
        //IDataBindingOutputReadonly<OutputType> WithReadonlyOutputType<OutputType>();
        //IDataBindingInput<InputType> WithInputType<InputType>();
    }

    public interface IChildDependendOutputBinding<out ChildOutputType> : IDataBinding
    {
        ChildOutputType GetChildDependendOutput(PathElement childPathElement);
    }

    /// <summary>
    /// Output interface for datacontexts.
    /// </summary>
    /// <remarks>
    /// Offers get functionality.
    /// </remarks>
    /// <typeparam name="OutputType">Type that is used if value get / set.</typeparam>
    public interface IDataBindingOutputReadonly<out OutputType> : IDataBinding
    {
        OutputType GetValue();
    }

    /// <summary>
    /// Output interface for datacontexts.
    /// </summary>
    /// <remarks>
    /// Offers get & set functionality.
    /// </remarks>
    /// <typeparam name="OutputType">Type that is used if value get / set.</typeparam>
    public interface IDataBindingOutput<OutputType> : IDataBinding
    {
        OutputType GetValue();
        void SetValue(OutputType value);
    }

    /// <summary>
    /// Input interface for datacontexts.
    /// </summary>
    /// <remarks>
    /// Offers get / set for the input type of the datacontext.
    /// </remarks>
    /// <typeparam name="InputType"></typeparam>
    public interface IDataBindingInput<InputType> : IDataBinding { }
    
    /// <summary>
    /// Fully specified datacontext.
    /// </summary>
    /// <typeparam name="InputType"></typeparam>
    /// <typeparam name="OutputType"></typeparam>
    public interface IDataBinding<InputType, OutputType> 
        : IDataBindingInput<InputType>, IDataBindingOutput<OutputType>
    { }



}