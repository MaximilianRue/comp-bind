using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CompBind;
using CompBind.ViewModel;

namespace CompBind.ViewModel
{
    public static class IDataBindingExtensions
    {
        /// <summary>
        /// Walks tree of bound properties upwards towards the root.
        /// Returns the absolute datapath of this binding.
        /// </summary>
        /// <param name="dataContext"></param>
        /// <returns></returns>
        public static Path GetAbsoluteBindingPath(this IDataBinding dataContext)
        {
            Path path = new Path();
            IDataBinding current = dataContext;
            while(current != null)
            {
                path.PathElements.AddFirst(current.LocalPath);
                current = current.Parent;
            }
            return path;
        }

        // THIS IS IGNORING GLOBAL FLAGS
        public static IDataBinding ResolvePath(
            this IDataBinding dataContext,
            Path path
        )
        {
            IDataBinding current = dataContext;
            foreach(PathElement pe in path.PathElements)
            {
                current = current.Step(pe);
            }
            return current;
        }
        public static IDataBinding ResolvePath(
            this IDataBinding dataContext,
            string pathString
        )
        {
            Path path = new Path(pathString);
            return dataContext.ResolvePath(path);
        }
    }
}
