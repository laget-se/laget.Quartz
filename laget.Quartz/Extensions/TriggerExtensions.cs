using Quartz;
using System.Globalization;

namespace laget.Quartz.Extensions
{
    public static class TriggerExtensions
    {
        public static string GetNextFireTime(this ITrigger trigger) => trigger.GetNextFireTimeUtc()?.DateTime.ToString(CultureInfo.CurrentCulture) ?? string.Empty;
    }
}
