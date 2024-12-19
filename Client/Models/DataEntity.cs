using Project.Interfaces;

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
