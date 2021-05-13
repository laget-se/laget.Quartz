using System;
using System.Collections.Specialized;
using System.Reflection;
using Autofac;
using Autofac.Extras.Quartz;
using Quartz;
using Quartz.Impl;

namespace laget.Quartz.Extensions
{
    public static class RegistrationExtensions
    {
        public static void RegisterQuartz(this ContainerBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }


            var config = new NameValueCollection
            {
                { "quartz.serializer.type", "binary" },
                { "quartz.scheduler.instanceName", Assembly.GetEntryAssembly().GetName().Name },
                { "quartz.jobStore.type", "Quartz.Simpl.RAMJobStore, Quartz" },
                { "quartz.threadPool.threadCount", "4" }
            };

            builder.RegisterQuartz(config);
        }

        public static void RegisterQuartz(this ContainerBuilder builder, NameValueCollection config)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }


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
