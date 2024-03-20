using Autofac;
using laget.Quartz.Modules;
using laget.Quartz.Utilities;
using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Specialized;
using System.Reflection;

namespace laget.Quartz.Extensions
{
    public static class ContainerBuilderExtensions
    {
        public static void RegisterQuartzJobs(this ContainerBuilder builder)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            var registrator = new Registrator(builder);
            registrator.TheCallingAssembly();
            registrator.Register();
        }

        public static void RegisterQuartzJobs(this ContainerBuilder builder, Action<IRegistrator> action)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            var registrator = new Registrator(builder);
            action(registrator);
            registrator.Register();
        }

        public static void RegisterQuartzService(this ContainerBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            var config = new NameValueCollection
            {
                { "quartz.serializer.type", "binary" },
                { "quartz.scheduler.instanceName", Assembly.GetEntryAssembly()?.GetName().Name },
                { "quartz.jobStore.type", "Quartz.Simpl.RAMJobStore, Quartz" },
                { "quartz.threadPool.threadCount", "4" }
            };

            builder.RegisterQuartzService(config);
        }

        public static void RegisterQuartzService(this ContainerBuilder builder, NameValueCollection config)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (config == null) throw new ArgumentNullException(nameof(config));

            builder.Register(c => new StdSchedulerFactory(config).GetScheduler().GetAwaiter().GetResult()).As<IScheduler>().SingleInstance();
            builder.RegisterModule(new QuartzAutofacFactoryModule
            {
                ConfigurationProvider = c => config
            });
            builder.RegisterModule(new QuartzAutofacJobsModule(Assembly.GetEntryAssembly()));

            builder.RegisterType<Service>().AsSelf().SingleInstance();
        }
    }
}
