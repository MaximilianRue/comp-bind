using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompBind.ViewModel
{
    /// <summary>
    /// Implements some very basic functionality of the DataBinding interface.
    /// </summary>
    public abstract class BaseBinding : IDataBinding
    {
        public virtual IDataBinding Parent { get; set; }
        public virtual PathElement LocalPath { get; set; }
        public virtual Scope ManagingScope { get; set; }
        public virtual BindingType BindingType { get; protected set; }

        public abstract IDataBinding Step(PathElement element);

        public virtual void NotifyDataBindingChanged()
        {
            ManagingScope.OnDataChanged(this);
        }

        public abstract IDataBinding Clone();
    }
}
