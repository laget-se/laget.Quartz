using Autofac;
using FluentAssertions;
using laget.Quartz.Factories;
using laget.Quartz.Modules;
using Moq;
using Quartz;
using Quartz.Impl;
using Quartz.Spi;
using System;
using System.Threading.Tasks;
using Xunit;

namespace laget.Quartz.Tests
{
    public class NestedScopeConfiguratorTests : IDisposable
    {
        private const string LocalScope = "local";
        private const string GlobalScope = "global";
        private readonly IContainer _container;
        private readonly AutofacJobFactory _jobFactory;

        public NestedScopeConfiguratorTests()
        {
            var cb = new ContainerBuilder();
            cb.RegisterType<SampleJob>();
            cb.Register(_ => new NestedDependency(GlobalScope)).InstancePerLifetimeScope();

            _container = cb.Build();

            _jobFactory = new AutofacJobFactory(_container.Resolve<ILifetimeScope>(),
                QuartzAutofacFactoryModule.LifetimeScopeName,
                (builder, tag) =>
                {
                    builder.Register(_ => new NestedDependency(LocalScope))
                        .InstancePerMatchingLifetimeScope(tag);
                });
        }

        public void Dispose()
        {
            _container.Dispose();
            GC.SuppressFinalize(this);
        }

        [Fact]
        public void ShouldApplyJobScopeConfiguration()
        {
            var jobDetail = new JobDetailImpl("test", typeof(SampleJob));
            var triggerBundle = new TriggerFiredBundle(
                jobDetail, Mock.Of<IOperableTrigger>(),
                Mock.Of<ICalendar>(), false,
                DateTimeOffset.UtcNow,
                null, null, null
            );

            var job = _jobFactory.NewJob(triggerBundle, Mock.Of<IScheduler>());
            job.As<SampleJob>().Dependency.Scope.Should().Be(LocalScope);

            _jobFactory.ReturnJob(job);
        }


        //[UsedImplicitly]
        [PersistJobDataAfterExecution]
        private class SampleJob : IJob
        {
            /// <summary>
            ///     Initializes a new instance of the <see cref="T:System.Object" /> class.
            /// </summary>
            public SampleJob(NestedDependency dependency)
            {
                Dependency = dependency ?? throw new ArgumentNullException(nameof(dependency));
            }

            public NestedDependency Dependency { get; }

            public Task Execute(IJobExecutionContext context) => Task.CompletedTask;
        }


        //[UsedImplicitly]
        private class NestedDependency
        {
            public NestedDependency(string scope)
            {
                if (string.IsNullOrEmpty(scope))
                    throw new ArgumentException("Value cannot be null or empty.", nameof(scope));
                Scope = scope;
            }

            /// <summary>
            ///     Scope.
            /// </summary>
            public string Scope { get; }
        }
    }
}
