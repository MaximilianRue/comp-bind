using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompBind.View
{
    public static class IValueUpdateReceiverExtensions
    {
        /// <summary>
        /// Returns the complete, absolute datapath of this update receiver.
        /// </summary>
        /// <remarks>
        /// Update receiver may be child to a metanode. In this case the local path
        /// is not the absolute one.
        /// </remarks>
        /// <param name="valueUpdateReceiver"></param>
        /// <returns>Absolute datapath of this update receiver.</returns>
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
