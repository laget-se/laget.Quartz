using Autofac;
using FluentAssertions;
using laget.Quartz.Factories;
using laget.Quartz.Modules;
using Quartz;
using Quartz.Impl;
using Quartz.Spi;
using System;
using System.Collections.Specialized;
using System.Threading.Tasks;
using Xunit;

namespace laget.Quartz.Tests
{
    public class QuartzAutofacFactoryModuleTests : IDisposable
    {
        private readonly IContainer _container;
        private readonly QuartzAutofacFactoryModule _quartzAutofacFactoryModule;


        public QuartzAutofacFactoryModuleTests()
        {
            var cb = new ContainerBuilder();
            _quartzAutofacFactoryModule = new QuartzAutofacFactoryModule();
            cb.RegisterModule(_quartzAutofacFactoryModule);

            _container = cb.Build();
        }

        public void Dispose()
        {
            _container.Dispose();
        }

        [Fact]
        public void CanUseGenericAutofacModuleRegistrationSyntax()
        {
            var cb = new ContainerBuilder();
            cb.RegisterModule<QuartzAutofacFactoryModule>();
            cb.Build();
        }

        [Fact]
        public void ShouldExecuteConfigureSchedulerFactoryFunctionIfSet()
        {
            var configuration = new NameValueCollection();
            var customSchedulerName = Guid.NewGuid().ToString();
            configuration[StdSchedulerFactory.PropertySchedulerInstanceName] = customSchedulerName;

            _quartzAutofacFactoryModule.ConfigurationProvider = _ => configuration;

            var scheduler = _container.Resolve<IScheduler>();
            scheduler.SchedulerName.Should().BeEquivalentTo(customSchedulerName);
        }

        [Fact]
        public void ShouldRegisterAutofacJobFactory()
        {
            _container.Resolve<AutofacJobFactory>().Should().NotBeNull();
            _container.Resolve<IJobFactory>().Should().BeOfType<AutofacJobFactory>();
            _container.Resolve<IJobFactory>().Should().BeSameAs(_container.Resolve<AutofacJobFactory>(),
                "should be singleton");
        }

        [Fact]
        public void ShouldRegisterAutofacSchedulerFactory()
        {
            var factory = _container.Resolve<ISchedulerFactory>();
            factory.Should().BeOfType<AutofacSchedulerFactory>();
        }

        [Fact]
        public void ShouldRegisterFactoryAsSingleton()
        {
            var factory = _container.Resolve<ISchedulerFactory>();
            _container.Resolve<ISchedulerFactory>().Should().BeSameAs(factory);
        }

        [Fact]
        public void ShouldRegisterSchedulerAsSingleton()
        {
            var scheduler = _container.Resolve<IScheduler>();
            _container.Resolve<IScheduler>().Should().BeSameAs(scheduler);
        }


        //[UsedImplicitly]
        private class TestJob : IJob
        {
            public Task Execute(IJobExecutionContext context)
            {
                return Task.CompletedTask;
            }
        }
    }
}
