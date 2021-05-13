# laget.Quartz
A generic implementation of Quartz, an open-source job scheduling system for .NET.

![Nuget](https://img.shields.io/nuget/v/laget.Caching)
![Nuget](https://img.shields.io/nuget/dt/laget.Caching)

## Configuration
> This implementation requires `Autofac` since this is the Inversion of Control container of our choosing.
### .NET Core
```c#
builder.RegisterQuartz();
```
```c#
builder.RegisterQuartz(new NameValueCollection
{
    { "quartz.serializer.type", "binary" },
    { "quartz.scheduler.instanceName", "ThisIsAnInstance" },
    { "quartz.jobStore.type", "Quartz.Simpl.RAMJobStore, Quartz" },
    { "quartz.threadPool.threadCount", "4" }
});
```

### .NET Framework
```c#
var builder = new ContainerBuilder();
builder.RegisterQuartz();
var container = builder.Build();
```
```c#
var builder = new ContainerBuilder();
builder.RegisterQuartz(new NameValueCollection
{
    { "quartz.serializer.type", "binary" },
    { "quartz.scheduler.instanceName", "ThisIsAnInstance" },
    { "quartz.jobStore.type", "Quartz.Simpl.RAMJobStore, Quartz" },
    { "quartz.threadPool.threadCount", "4" }
});
var container = builder.Build();
```
> For a full configuration reference, please take a look at [here!](https://www.quartz-scheduler.net/documentation/quartz-3.x/configuration/reference.html#main-configuration)