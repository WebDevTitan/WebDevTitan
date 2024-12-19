using System;

namespace Project.Interfaces
{
    interface ICloseable
    {
        event EventHandler<EventArgs> RequestClose;
        event EventHandler<EventArgs> RequestMinimize;
        event EventHandler<EventArgs> RequestRestore;
    }
}
