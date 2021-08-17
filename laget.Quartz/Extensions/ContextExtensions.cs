using System.Globalization;
using Quartz;

namespace laget.Quartz.Extensions
{
    public static class ContextExtensions
    {
        public static string GetFireTime(this IJobExecutionContext context) => context.FireTimeUtc.LocalDateTime.ToString(CultureInfo.CurrentCulture);
        public static string GetNextFireTime(this IJobExecutionContext context) => context.NextFireTimeUtc?.DateTime.ToLocalTime().ToString(CultureInfo.CurrentCulture) ?? string.Empty;
    }
}
