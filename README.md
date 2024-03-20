# laget.Quartz
A generic implementation of Quartz, an open-source job scheduling system for .NET.

![Nuget](https://img.shields.io/nuget/v/laget.Quartz)
![Nuget](https://img.shields.io/nuget/dt/laget.Quartz)

## Configuration
> This implementation requires `Autofac` since this is the Inversion of Control container of our choosing.

### Usage
#### Simple
> This will by default register all jobs from the calling assembly.
```c#
await Host.CreateDefaultBuilder()
    .ConfigureContainer<ContainerBuilder>((context, builder) =>
    {
        builder.RegisterQuartzJobs();
        builder.RegisterQuartzService();
    })
    .ConfigureServices((context, services) =>
    {
        services.AddQuartzService();
    })
    .Build()
    .RunAsync();
```

### Advanced
```c#
await Host.CreateDefaultBuilder()
    .ConfigureContainer<ContainerBuilder>((context, builder) =>
    {
        builder.RegisterQuartzJobs(_ =>
        {
            _.Assembly("name");
            _.TheCallingAssembly();
            _.TheEntryAssembly();
            _.TheExecutingAssembly();
        });
        builder.RegisterQuartzService(new NameValueCollection
        {
            { "quartz.serializer.type", "binary" },
            { "quartz.scheduler.instanceName", "ThisIsAnInstance" },
            { "quartz.jobStore.type", "Quartz.Simpl.RAMJobStore, Quartz" },
            { "quartz.threadPool.threadCount", "4" }
        });
    })
    .ConfigureServices((context, services) =>
    {
        services.AddQuartzService();
    })
    .Build()
    .RunAsync();
```


This is the advanced and more customizable way to register your mappers

* `Assembly("name");`
  > This will register mapper implementations in the assembly, will load assembly via the name provided using `System.Reflection`, that inherits from `Job (laget.Quartz.Job)`.
* `TheCallingAssembly();`
  > This will register mapper implementations from the calling assembly that inherits from `Job (laget.Quartz.Job)`.
* `TheEntryAssembly();`
  > This will register mapper implementations from the entry assembly that inherits from `Job (laget.Quartz.Job)`.
* `TheExecutingAssembly();`
  > This will register mapper implementations from the executing assembly that inherits from `Job (laget.Quartz.Job)`.

> For a full configuration reference, please take a look at [here!](https://www.quartz-scheduler.net/documentation/quartz-3.x/configuration/reference.html#main-configuration)

#### Job
```c#
[DisallowConcurrentExecution]
public class SomeJob : laget.Quartz.Job
{
    private readonly ISomeService _someService;

    // We need this empty constructor as it's used when scheduling the job
    public SomeJob()
    {
    }

    public SomeJob(ISomeService someService)
    {
        _someService = someService;
    }

    public override async Task ExecuteJob(IJobExecutionContext context)
    {
        // Do some stuff here!
    }


    public override ITrigger Trigger => TriggerBuilder
        .Create()
        .StartNow()
        .WithIdentity(Name, Group)
        .WithSimpleSchedule(x => x
            .WithInterval(Interval)
            .WithMisfireHandlingInstructionIgnoreMisfires()
            .RepeatForever()
        )
        .Build();

    private static TimeSpan Interval => TimeSpan.FromSeconds(60);
    public override string Group => "Group";
    public override string Name => nameof(SomeJob);
}
```
