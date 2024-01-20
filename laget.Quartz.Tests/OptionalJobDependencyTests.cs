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
    public class OptionalJobDependencyTests : IDisposable

    {
        private readonly ContainerBuilder _containerBuilder;


        private IContainer _container;

        public OptionalJobDependencyTests()
        {
            _containerBuilder = new ContainerBuilder();
            _containerBuilder.RegisterType<JobDependency>().As<IJobDependency>();
        }

        public void Dispose()
        {
            _container?.Dispose();
        }

        [Fact]
        public void ShouldIgnoreRegisteredOptionalDependencies_UnlessExplicitlyConfigured()
        {
            _containerBuilder.RegisterModule(new QuartzAutofacJobsModule(Assembly.GetExecutingAssembly()));
            _container = _containerBuilder.Build();

            var job = _container.Resolve<TestJobWithOptionalDependency>();
            job.Dependency.Should().BeNull();
        }

        [Fact]
        public void ShouldWireRegisteredOptionalDependencies()
        {
            _containerBuilder.RegisterModule(new QuartzAutofacJobsModule(Assembly.GetExecutingAssembly())
            {
                AutoWireProperties = true
            });
            _container = _containerBuilder.Build();


            var job = _container.Resolve<TestJobWithOptionalDependency>();
            job.Dependency.Should().NotBeNull("should wire optional dependency");
        }

        //[UsedImplicitly]
        private class TestJobWithOptionalDependency : IJob
        {
            public IJobDependency Dependency { get; set; }

            public Task Execute(IJobExecutionContext context) => Task.CompletedTask;
        }

        private interface IJobDependency
        {
        }

        //[UsedImplicitly]
        private class JobDependency : IJobDependency
        {
        }
    }
}
