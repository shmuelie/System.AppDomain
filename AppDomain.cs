using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;
using System.Linq.Expressions;

namespace System
{
    public sealed class AppDomain
    {
        internal static readonly Type RealType = typeof(string).GetTypeInfo().Assembly.GetType("System.AppDomain");
        internal static readonly Type RealResolveEventHandler = typeof(string).GetTypeInfo().Assembly.GetType("System.ResolveEventHandler");
        private static readonly Lazy<AppDomain> currentDomain;
        private static readonly MethodInfo assemblyResolveAdd;
        private static readonly MethodInfo assemblyResolveRemove;

        static AppDomain()
        {
            currentDomain = new Lazy<AppDomain>(CreateCurrentDomain);
            AddEventMethods("AssemblyResolve", out assemblyResolveAdd, out assemblyResolveRemove);
        }

        private static void AddEventMethods(string eventName, out MethodInfo add, out MethodInfo remove)
        {
            EventInfo eventInfo = RealType.GetEvent(eventName, BindingFlags.FlattenHierarchy | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            add = eventInfo.AddMethod;
            remove = eventInfo.RemoveMethod;
        }

        private static AppDomain CreateCurrentDomain()
        {
            return new AppDomain(RealType.GetProperty(nameof(CurrentDomain), BindingFlags.DeclaredOnly | BindingFlags.Static | BindingFlags.Public).GetValue(null));
        }

        public static AppDomain CurrentDomain
        {
            get
            {
                return currentDomain.Value;
            }
        }

        private readonly object appDomain;
        private readonly Delegate assemblyResolveReal;
        private ResolveEventHandler assemblyResolve;

        internal AppDomain(object appDomain)
        {
            if (ReferenceEquals(appDomain, null))
            {
                throw new ArgumentNullException(nameof(appDomain), $"'{nameof(appDomain)}' cannot be null (Nothing in Visual Basic)");
            }
            if (!RealType.IsInstanceOfType(appDomain))
            {
                throw new ArgumentException($"'{nameof(appDomain)}' must be a real System.AppDomain", nameof(appDomain));
            }
            this.appDomain = appDomain;
            assemblyResolveReal = CreateResolveEventHandler(args => OnAssemblyResolve(args));
        }

        private Delegate CreateResolveEventHandler(Expression<Func<ResolveEventArgs, Assembly>> onResolve)
        {
            ParameterExpression argsParameter = Expression.Parameter(ResolveEventArgs.RealType, "args");
            return Expression.Lambda(RealResolveEventHandler, Expression.Invoke(onResolve, Enumerable.Repeat(Expression.New(typeof(ResolveEventArgs).GetConstructor(new[] { typeof(object) }), Enumerable.Repeat(argsParameter, 1)), 1)), Expression.Parameter(typeof(object), "sender"), argsParameter).Compile();
        }

        private void EventWrapper(MulticastDelegate @delegate, Delegate realDelegate, MethodInfo manipulationInfo)
        {
            if (@delegate == null || @delegate.GetInvocationList().Length == 0)
            {
                manipulationInfo.Invoke(appDomain, new[] { realDelegate });
            }
        }

        private Assembly OnAssemblyResolve(ResolveEventArgs args)
        {
            return assemblyResolve?.Invoke(this, args);
        }

        public event ResolveEventHandler AssemblyResolve
        {
            add
            {
                EventWrapper(assemblyResolve, assemblyResolveReal, assemblyResolveAdd);
                assemblyResolve = (ResolveEventHandler)Delegate.Combine(assemblyResolve, value);
            }
            remove
            {
                assemblyResolve = (ResolveEventHandler)Delegate.Remove(assemblyResolve, value);
                EventWrapper(assemblyResolve, assemblyResolveReal, assemblyResolveRemove);
            }
        }
    }
}
