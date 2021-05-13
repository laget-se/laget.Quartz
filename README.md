# laget.Quartz
A generic implementation of Quartz, an open-source job scheduling system for .NET.

![Nuget](https://img.shields.io/nuget/v/laget.Quartz)
![Nuget](https://img.shields.io/nuget/dt/laget.Quartz)

## Configuration
> This implementation requires `Autofac` since this is the Inversion of Control container of our choosing.

### Usage
```c#
await Host.CreateDefaultBuilder()
    .ConfigureContainer<ContainerBuilder>((context, builder) =>
    {
        builder.RegisterQuartz();
    })
    .ConfigureServices((context, services) =>
    {
        services.AddQuartzService();
    })
    .Build()
    .RunAsync();
```
```c#
await Host.CreateDefaultBuilder()
    .ConfigureContainer<ContainerBuilder>((context, builder) =>
    {
        builder.RegisterQuartz(new NameValueCollection
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

> For a full configuration reference, please take a look at [here!](https://www.quartz-scheduler.net/documentation/quartz-3.x/configuration/reference.html#main-configuration)