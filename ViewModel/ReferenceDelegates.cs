using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompBind.ViewModel
{
    public delegate void ActionRef<TypeA>(ref TypeA A);
    public delegate void ActionRef<TypeA, TypeB>(ref TypeA A, TypeB B);
}
