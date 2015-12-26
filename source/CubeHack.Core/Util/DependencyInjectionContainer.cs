// Copyright (c) the CubeHack authors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt in the project root.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CubeHack.Util
{
    /// <summary>
    /// Implements a very basic dependency injection container that can resolve types with exactly one public constructor.
    /// </summary>
    public sealed class DependencyInjectionContainer : IDisposable
    {
        private const BindingFlags _bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        private readonly object _mutex = new object();
        private readonly Dictionary<Type, object> _instances = new Dictionary<Type, object>();
        private readonly HashSet<Type> _currentlyCreatingInstances = new HashSet<Type>(); // Used to detect mutual recursion.

        private List<IDisposable> _instancesToDispose;

        public DependencyInjectionContainer()
        {
        }

        /// <summary>
        /// Disposes (if applicable) and forgets every object ever created by this DependencyInjectionContainer.
        /// </summary>
        public void Dispose()
        {
            List<IDisposable> instancesToDispose;

            lock (_mutex)
            {
                instancesToDispose = _instancesToDispose;
                _instancesToDispose = null;
                _instances.Clear();
            }

            if (instancesToDispose != null)
            {
                for (int i = instancesToDispose.Count - 1; i >= 0; --i)
                {
                    try
                    {
                        instancesToDispose[i].Dispose();
                    }
                    catch
                    {
                    }
                }
            }
        }

        /// <summary>
        /// Resolves the specified type and returns its (singleton) instance.
        /// </summary>
        /// <typeparam name="T">The type to resolve.</typeparam>
        /// <returns>The resolved instance.</returns>
        public T Resolve<T>()
        {
            return (T)Resolve(typeof(T));
        }

        private object Resolve(Type type)
        {
            lock (_mutex)
            {
                object instance;
                if (_instances.TryGetValue(type, out instance)) return instance;

                if (_currentlyCreatingInstances.Contains(type))
                {
                    throw new Exception($"Recursion detected when resolving type '{type.FullName}'");
                }

                _currentlyCreatingInstances.Add(type);
                try
                {
                    var constructors = type.GetConstructors(_bindingFlags).Where(c => c.GetCustomAttributes<DependencyInjectedAttribute>().Any()).ToList();
                    if (constructors.Count != 1) throw new Exception($"Type needs exactly one constructor with [{nameof(DependencyInjectedAttribute)}]: '{type.FullName}'");
                    var constructor = constructors[0];

                    var parameters = constructor.GetParameters();
                    var arguments = new object[parameters.Length];
                    for (int i = 0; i < parameters.Length; ++i)
                    {
                        arguments[i] = Resolve(parameters[i].ParameterType);
                    }

                    instance = constructor.Invoke(arguments);

                    var disposableInstance = instance as IDisposable;
                    if (disposableInstance != null)
                    {
                        if (_instancesToDispose == null) _instancesToDispose = new List<IDisposable>();
                        _instancesToDispose.Add(disposableInstance);
                    }

                    _instances.Add(type, instance);

                    return instance;
                }
                finally
                {
                    _currentlyCreatingInstances.Remove(type);
                }
            }
        }
    }
}
