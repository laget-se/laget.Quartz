using Autofac;
using FluentAssertions;
using laget.Quartz.Modules;
using Quartz;
using System;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;

namespace laget.Quartz.Tests
{
    public class QuartzAutofacJobsModuleTests : IDisposable
    {
        private IContainer _container;

        public void Dispose()
        {
            _container?.Dispose();
        }

        [Fact]
        public void ShouldApplyJobRegistrationFilter()
        {
            var builder = new ContainerBuilder();
            builder.RegisterModule(new QuartzAutofacJobsModule(Assembly.GetExecutingAssembly())
            {
                JobFilter = type => type != typeof(TestJob2)
            });
            _container = builder.Build();

            _container.IsRegistered<TestJob2>().Should().BeFalse();
        }

        [Fact]
        public void ShouldRegisterAllJobsFromAssembly()
        {
            var builder = new ContainerBuilder();
            builder.RegisterModule(new QuartzAutofacJobsModule(Assembly.GetExecutingAssembly()));
            _container = builder.Build();

            _container.IsRegistered<TestJob>()
                .Should().BeTrue();
        }


        //[UsedImplicitly]
        private class TestJob2 : IJob
        {
            public Task Execute(IJobExecutionContext context) => Task.CompletedTask;
        }


        //[UsedImplicitly]
        private class TestJob : IJob
        {
            public Task Execute(IJobExecutionContext context) => Task.CompletedTask;
        }
    }
}
