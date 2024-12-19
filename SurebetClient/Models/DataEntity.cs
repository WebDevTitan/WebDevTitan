using Project.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Models
{
    public abstract class DataEntity : IClonable
    {
        public virtual object Clone()
        {
            return MemberwiseClone();
        }
    }
}
