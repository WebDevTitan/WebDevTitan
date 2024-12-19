using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Interfaces
{
    interface ICloseable
    {
        event EventHandler<EventArgs> RequestClose;
        event EventHandler<EventArgs> RequestMinimize;
        event EventHandler<EventArgs> RequestRestore;
    }
}
