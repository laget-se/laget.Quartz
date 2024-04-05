using laget.Quartz.Utilities;

namespace laget.Quartz
{
    public abstract class Module
    {
        public abstract void Configure(IRegistrator registrator);
    }
}
