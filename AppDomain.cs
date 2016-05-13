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
        internal static readonly Type RealType = typeof(string).GetTypeInfo().Assembly.GetType(typeof(AppDomain).FullName);
        internal static readonly Type RealResolveEventHandler = typeof(string).GetTypeInfo().Assembly.GetType(typeof(ResolveEventHandler).FullName);
        private static readonly Lazy<AppDomain> currentDomain;
        private static readonly MethodInfo assemblyResolveAdd;
        private static readonly MethodInfo assemblyResolveRemove;
        private static readonly MethodInfo typeResolveAdd;
        private static readonly MethodInfo typeResolveRemove;
        private static readonly MethodInfo resourceResolveAdd;
        private static readonly MethodInfo resourceResolveRemove;
        private static readonly ConstructorInfo resolveEventArgsConstructor;
        private static readonly Func<object, string> getBaseDirectory;

        static AppDomain()
        {
            resolveEventArgsConstructor = typeof(ResolveEventArgs).GetConstructor(new[] { typeof(object) });
            currentDomain = new Lazy<AppDomain>(CreateCurrentDomain);
            AddEventMethods(nameof(AssemblyResolve), out assemblyResolveAdd, out assemblyResolveRemove);
            AddEventMethods(nameof(TypeResolve), out typeResolveAdd, out typeResolveRemove);
            AddEventMethods(nameof(ResourceResolve), out resourceResolveAdd, out resourceResolveRemove);
            ParameterExpression baseDirectoryParameter = Expression.Parameter(typeof(object), nameof(appDomain));
            getBaseDirectory = Expression.Lambda<Func<object, string>>(Expression.Property(Expression.Convert(baseDirectoryParameter, RealType), RealType.GetProperty(nameof(BaseDirectory), BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public).GetMethod), true, Enumerable.Repeat(baseDirectoryParameter, 1)).Compile();
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
        private readonly Delegate typeResolveReal;
        private ResolveEventHandler typeResolve;
        private readonly Delegate resourceResolveReal;
        private ResolveEventHandler resourceResolve;

        internal AppDomain(object appDomain)
        {
            appDomain.NotNull(nameof(appDomain));
            if (!RealType.IsInstanceOfType(appDomain))
            {
                throw new ArgumentException($"'{nameof(appDomain)}' must be a real {typeof(AppDomain).FullName}", nameof(appDomain));
            }
            this.appDomain = appDomain;
            assemblyResolveReal = CreateResolveEventHandler(nameof(OnAssemblyResolve));
            typeResolveReal = CreateResolveEventHandler(nameof(OnTypeResolve));
            resourceResolveReal = CreateResolveEventHandler(nameof(OnResourceResolve));
        }

        public string BaseDirectory
        {
            get
            {
                return getBaseDirectory(appDomain);
            }
        }

        private Delegate CreateResolveEventHandler(string methodName)
        {
            ParameterExpression eParameter = Expression.Parameter(typeof(ResolveEventArgs), "e");
            ParameterExpression argsParameter = Expression.Parameter(ResolveEventArgs.RealType, "args");
            return Expression.Lambda(
                RealResolveEventHandler,
                Expression.Invoke(
                    Expression.Lambda<Func<ResolveEventArgs, Assembly>>(
                        Expression.Call(
                            Expression.Constant(
                                this, typeof(AppDomain)),
                            typeof(AppDomain).GetMethod(
                                methodName,
                                BindingFlags.FlattenHierarchy | BindingFlags.Instance | BindingFlags.NonPublic),
                            Enumerable.Repeat(eParameter, 1)),
                        Enumerable.Repeat(
                            eParameter,
                            1)),
                    Enumerable.Repeat(
                        Expression.New(
                            resolveEventArgsConstructor,
                            Enumerable.Repeat(
                                argsParameter,
                                1)),
                        1)),
                false,
                (new ParameterExpression[] {
                    Expression.Parameter(
                        typeof(object),
                        "sender"
                    ),
                    argsParameter
                }).AsEnumerable()
                ).Compile();
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

        private Assembly OnTypeResolve(ResolveEventArgs args)
        {
            return typeResolve?.Invoke(this, args);
        }

        public event ResolveEventHandler TypeResolve
        {
            add
            {
                EventWrapper(typeResolve, typeResolveReal, typeResolveAdd);
                typeResolve = (ResolveEventHandler)Delegate.Combine(typeResolve, value);
            }
            remove
            {
                typeResolve = (ResolveEventHandler)Delegate.Remove(typeResolve, value);
                EventWrapper(typeResolve, typeResolveReal, typeResolveRemove);
            }
        }

        private Assembly OnResourceResolve(ResolveEventArgs args)
        {
            return resourceResolve?.Invoke(this, args);
        }

        public event ResolveEventHandler ResourceResolve
        {
            add
            {
                EventWrapper(resourceResolve, resourceResolveReal, resourceResolveAdd);
                resourceResolve = (ResolveEventHandler)Delegate.Combine(resourceResolve, value);
            }
            remove
            {
                resourceResolve = (ResolveEventHandler)Delegate.Remove(resourceResolve, value);
                EventWrapper(resourceResolve, resourceResolveReal, resourceResolveRemove);
            }
        }
    }
}
