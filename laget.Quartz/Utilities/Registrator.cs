using Autofac;
using laget.Quartz.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace laget.Quartz.Utilities
{
    public interface IRegistrator
    {
        /// <summary>
        /// Add an Assembly to the scanning operation.
        /// </summary>
        /// <param name="assembly"></param>
        void Assembly(Assembly assembly);

        /// <summary>
        /// Add an Assembly by name to the scanning operation.
        /// </summary>
        /// <param name="assemblyName"></param>
        void Assembly(string assemblyName);

        /// <summary>
        /// Adds a single Job.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        void Register<T>();

        /// <summary>
        /// Add the calling Assembly to the scanning operation
        /// </summary>
        void TheCallingAssembly();

        /// <summary>
        /// Add the entry Assembly to the scanning operation
        /// </summary>
        void TheEntryAssembly();

        /// <summary>
        /// Add the executing Assembly to the scanning operation
        /// </summary>
        void TheExecutingAssembly();
    }

    public class Registrator : IRegistrator
    {
        private readonly Dictionary<int, TypeReference> _bindings = new Dictionary<int, TypeReference>();
        private readonly ContainerBuilder _builder;

        public Registrator(ContainerBuilder builder)
        {
            _builder = builder;
        }

        public void Assembly(Assembly assembly)
        {
            var jobs = assembly?.DefinedTypes.Where(t => t.BaseType == typeof(Job) && !t.IsDefined(typeof(DisableRegistrationAttribute), false)).ToList();

            foreach (var job in jobs)
            {
                _bindings.Add(job.GetHashCode(), new TypeReference(assembly, job));
            }
        }

        public void Assembly(string name)
        {
            Assembly(System.Reflection.Assembly.Load(new AssemblyName(name)), typeof(Job));
        }

        public void Register<T>()
        {
            var type = typeof(T);
            var assembly = type.GetTypeInfo().Assembly;

            _bindings.Add(type.GetHashCode(), new TypeReference(assembly, type));
        }

        public void TheCallingAssembly()
        {
            Assembly(System.Reflection.Assembly.GetEntryAssembly(), typeof(Job));
        }

        public void TheEntryAssembly()
        {
            Assembly(System.Reflection.Assembly.GetEntryAssembly(), typeof(Job));
        }

        public void TheExecutingAssembly()
        {
            Assembly(System.Reflection.Assembly.GetExecutingAssembly(), typeof(Job));
        }

        private void Assembly(Assembly assembly, Type type)
        {
            var jobs = assembly?.DefinedTypes.Where(t => t.BaseType == type && !t.IsDefined(typeof(DisableRegistrationAttribute), false)).ToList();

            foreach (var job in jobs)
            {
                _bindings.Add(job.GetHashCode(), new TypeReference(assembly, job));
            }
        }


        /// <summary>
        /// Register all added type instances to Autofac.
        /// </summary>
        public void Register()
        {
            foreach (var kvp in _bindings)
            {
                _builder.RegisterAssemblyTypes(kvp.Value.Assembly).AssignableTo(kvp.Value.Type).As<Job>().SingleInstance();
            }
        }

        private class TypeReference
        {
            public TypeReference(Assembly assembly, Type type)
            {
                Assembly = assembly;
                Type = type;
            }

            public Assembly Assembly { get; }
            public Type Type { get; }
        }
    }
}
