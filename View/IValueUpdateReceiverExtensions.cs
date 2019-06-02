using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompBind.View
{
    public static class IValueUpdateReceiverExtensions
    {
        public static Path GetAbsoluteDataPath(this IValueUpdateReceiver valueUpdateReceiver)
        {
            // Start  with local path of update receiver
            Path absolutePath = new Path(valueUpdateReceiver.BindingPath);

            // Prepend path of potential MetaDataNode parenting this value update receiver
            if(valueUpdateReceiver.Context is IMetaDataNode)
            {
                IMetaDataNode metaDataNode = valueUpdateReceiver.Context as IMetaDataNode;
                absolutePath = Path.Concatenate(metaDataNode.GetAbsoluteDataPath(), absolutePath);
            }

            return absolutePath;
        }
    }
}
