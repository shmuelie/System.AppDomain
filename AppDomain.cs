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
        private static readonly Func<object> getCurrentDomain;
        private static readonly Lazy<AppDomain> currentDomain;
        private static readonly MethodInfo assemblyResolveAdd;
        private static readonly MethodInfo assemblyResolveRemove;

        static AppDomain()
        {
            getCurrentDomain = Expression.Lambda<Func<object>>(Expression.Property(null, RealType.GetProperty("CurrentDomain", BindingFlags.DeclaredOnly | BindingFlags.Static | BindingFlags.Public)), true, Enumerable.Repeat<ParameterExpression>(null, 0)).Compile();
            currentDomain = new Lazy<AppDomain>(CreateCurrentDomain);
            EventInfo assemblyResolveEvent = RealType.GetEvent("AssemblyResolve", BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy | BindingFlags.NonPublic);
            assemblyResolveAdd = assemblyResolveEvent.AddMethod;
            assemblyResolveRemove = assemblyResolveEvent.RemoveMethod;
        }

        private static AppDomain CreateCurrentDomain()
        {
            return new AppDomain(getCurrentDomain());
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
            ParameterExpression argsParameter = Expression.Parameter(ResolveEventArgs.RealType, "args");
            Expression<Func<ResolveEventArgs, Assembly>> onAssemblyResolve = args => OnAssemblyResolve(this, args);
            assemblyResolveReal = Expression.Lambda(RealResolveEventHandler, Expression.Invoke(onAssemblyResolve, Enumerable.Repeat(Expression.New(typeof(ResolveEventArgs).GetConstructor(new[] { typeof(object) }), Enumerable.Repeat(argsParameter, 1)), 1)), Expression.Parameter(typeof(object), "sender"), argsParameter).Compile();
        }

        private Assembly OnAssemblyResolve(object sender, ResolveEventArgs args)
        {
            return assemblyResolve?.Invoke(sender, args);
        }

        public event ResolveEventHandler AssemblyResolve
        {
            add
            {
                if (assemblyResolve == null || assemblyResolve.GetInvocationList().Length == 0)
                {
                    assemblyResolveAdd.Invoke(appDomain, new[] { assemblyResolveReal });
                }
                assemblyResolve = (ResolveEventHandler)Delegate.Combine(assemblyResolve, value);
            }
            remove
            {
                assemblyResolve = (ResolveEventHandler)Delegate.Remove(assemblyResolve, value);
                if (assemblyResolve == null || assemblyResolve.GetInvocationList().Length == 0)
                {
                    assemblyResolveRemove.Invoke(appDomain, new[] { assemblyResolveReal });
                }
            }
        }
    }
}
