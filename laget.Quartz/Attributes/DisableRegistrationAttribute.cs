using System;

namespace laget.Quartz.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
    public class DisableRegistrationAttribute : Attribute
    {
    }
}
