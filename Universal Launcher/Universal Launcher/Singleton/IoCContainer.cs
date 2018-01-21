using System;
using System.Collections.Generic;
using System.Linq;

namespace Universal_Launcher.Singleton
{
    public class IoCContainer
    {
        private static IoCContainer _instanse;


        private readonly Dictionary<Type, Func<object>> _registrations = new Dictionary<Type, Func<object>>();
        public static IoCContainer Instanse => _instanse ?? (_instanse = new IoCContainer());

        public void Register<TService, TImpl>() where TImpl : TService
        {
            _registrations.Add(typeof(TService), () => GetInstance(typeof(TImpl)));
        }

        public void Register<TService>(Func<TService> instanceCreator)
        {
            _registrations.Add(typeof(TService), () => instanceCreator());
        }

        public void RegisterSingleton<TService>(TService instance)
        {
            _registrations.Add(typeof(TService), () => instance);
        }

        public void RegisterSingleton<TService>(Func<TService> instanceCreator)
        {
            var lazy = new Lazy<TService>(instanceCreator);
            Register(() => lazy.Value);
        }

        public object GetInstance(Type serviceType)
        {
            foreach (var registration in _registrations)
                if (registration.Key == serviceType
                    || serviceType.IsAssignableFrom(registration.Key))
                    return registration.Value();

            if (!serviceType.IsAbstract)
                return CreateInstance(serviceType);

            throw new InvalidOperationException("No registration for " + serviceType);
        }

        public T Resolve<T>()
        {
            return (T) GetInstance(typeof(T));
        }

        private object CreateInstance(Type implementationType)
        {
            var ctor = implementationType.GetConstructors().First();
            var parameterTypes = ctor.GetParameters().Select(p => p.ParameterType);
            var dependencies = parameterTypes.Select(GetInstance).ToArray();
            return Activator.CreateInstance(implementationType, dependencies);
        }
    }
}