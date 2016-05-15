using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace System
{
    /// <summary>
    ///     Represents an application domain, which is an isolated environment where applications execute. This class cannot be inherited.
    /// </summary>
    /// <remarks>
    ///     The AppDomain class implements a set of events that enable applications to respond when an assembly is loaded.
    /// </remarks>
    /// <threadsafety static="true" instance="false"/>
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
#pragma warning disable HeapAnalyzerImplicitParamsRule // Array allocation for params parameter
            return new AppDomain(Expression.Lambda<Func<object>>(Expression.Property(null, RealType.GetProperty(nameof(CurrentDomain), BindingFlags.DeclaredOnly | BindingFlags.Static | BindingFlags.Public).GetMethod)).Compile()());
#pragma warning restore HeapAnalyzerImplicitParamsRule // Array allocation for params parameter
        }

        /// <summary>
        ///     Gets the current application domain for the current application.
        /// </summary>
        /// <value>
        ///     The current application domain.
        /// </value>
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

        /// <summary>
        ///     Gets the base directory that the assembly resolver uses to probe for assemblies.
        /// </summary>
        /// <value>
        ///     The base directory that the assembly resolver uses to probe for assemblies.
        /// </value>
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

        /// <summary>
        ///     Occurs when the resolution of an assembly fails.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         It is the responsibility of the <see cref="ResolveEventHandler"/> for this event to return the assembly that is specified by the <see cref="ResolveEventArgs.Name"/> property, or to return <see langword="null"/> if the assembly is not recognized.
        ///     </para>
        ///     <para>
        ///         For guidance on the use of this event, see <see href="https://msdn.microsoft.com/en-us/library/ff527268(v=vs.110).aspx">Resolving Assembly Loads</see>.
        ///     </para>
        /// </remarks>
        /// <seealso cref="ResolveEventArgs.RequestingAssembly"/>
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

        /// <summary>
        ///     Occurs when the resolution of a type fails.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         The TypeResolve event occurs when the common language runtime is unable to determine the assembly that can create the requested type. This can occur if the type is defined in a dynamic assembly, or the type is not defined in a dynamic assembly but the runtime does not know which assembly the type is defined in. The latter situation can occur when <see cref="Type.GetType(string)"/> is called with a type name that is not qualified with the assembly name.
        ///     </para>
        ///     <para>
        ///         The <see cref="ResolveEventHandler"/> for this event can attempt to locate and create the type.
        ///     </para>
        ///     <para>
        ///         However, the TypeResolve event does not occur if the runtime knows it is not possible to find a type in certain assemblies. For example, this event does not occur if the type is not found in a static assembly because the runtime knows types cannot be added dynamically to static assemblies.
        ///     </para>
        /// </remarks>
        /// <seealso cref="ResolveEventArgs.RequestingAssembly"/>
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

        /// <summary>
        ///     Occurs when the resolution of a resource fails because the resource is not a valid linked or embedded resource in the assembly.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         The <see cref="ResolveEventHandler"/> for this event can attempt to locate the assembly containing the resource and return it.
        ///     </para>
        ///     <note type="important">
        ///         This event is not raised if resolution fails because no file can be found for a valid linked resource. It is raised if a manifest resource stream cannot be found, but it is not raised if an individual resource key cannot be found.
        ///     </note>
        /// </remarks>
        /// <seealso cref="ResolveEventArgs.RequestingAssembly"/>
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
