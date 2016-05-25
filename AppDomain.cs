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
        internal static readonly Type RealType = typeof(AppDomain).RealType();
        internal static readonly Type RealResolveEventHandler = typeof(ResolveEventHandler).RealType();
        internal static readonly Type RealUnhandledExceptionEventHandler = typeof(UnhandledExceptionEventHandler).RealType();
        private static readonly Lazy<AppDomain> currentDomain;
        private static readonly MethodInfo assemblyResolveAdd;
        private static readonly MethodInfo assemblyResolveRemove;
        private static readonly MethodInfo typeResolveAdd;
        private static readonly MethodInfo typeResolveRemove;
        private static readonly MethodInfo resourceResolveAdd;
        private static readonly MethodInfo resourceResolveRemove;
        private static readonly MethodInfo unhandledExceptionAdd;
        private static readonly MethodInfo unhandledExceptionRemove;
        private static readonly Func<object, string> getBaseDirectory;

        static AppDomain()
        {
#pragma warning disable HeapAnalyzerExplicitNewObjectRule // Explicit new reference type allocation
            currentDomain = new Lazy<AppDomain>(CreateCurrentDomain);
#pragma warning restore HeapAnalyzerExplicitNewObjectRule // Explicit new reference type allocation
            RealType.GetEventMethods(nameof(AssemblyResolve), out assemblyResolveAdd, out assemblyResolveRemove);
            RealType.GetEventMethods(nameof(TypeResolve), out typeResolveAdd, out typeResolveRemove);
            RealType.GetEventMethods(nameof(ResourceResolve), out resourceResolveAdd, out resourceResolveRemove);
#if DNX
            RealType.GetEventMethods(nameof(UnhandledException), out unhandledExceptionAdd, out unhandledExceptionRemove);
#endif
            getBaseDirectory = RealType.GetInstancePropertyFunction<string>(nameof(BaseDirectory));
        }

#pragma warning disable HeapAnalyzerExplicitNewObjectRule // Explicit new reference type allocation
        private static AppDomain CreateCurrentDomain() => new AppDomain(RealType.GetStaticPropertyFunction(nameof(CurrentDomain))());
#pragma warning restore HeapAnalyzerExplicitNewObjectRule // Explicit new reference type allocation

        /// <summary>
        ///     Gets the current application domain for the current application.
        /// </summary>
        /// <value>
        ///     The current application domain.
        /// </value>
        public static AppDomain CurrentDomain => currentDomain.Value;

        private readonly object appDomain;
        private readonly Delegate assemblyResolveReal;
        private ResolveEventHandler assemblyResolve;
        private readonly Delegate typeResolveReal;
        private ResolveEventHandler typeResolve;
        private readonly Delegate resourceResolveReal;
        private ResolveEventHandler resourceResolve;
        private readonly Delegate unhandledExceptionReal;
        private UnhandledExceptionEventHandler unhandledException;

        internal AppDomain(object appDomain)
        {
            appDomain.NotNull(nameof(appDomain));
            appDomain.InstanceOf(nameof(appDomain), RealType);
            this.appDomain = appDomain;
            assemblyResolveReal = this.CreateEventDelegate<ResolveEventArgs, Assembly>(nameof(OnAssemblyResolve), ResolveEventArgs.RealType, RealResolveEventHandler);
            typeResolveReal = this.CreateEventDelegate<ResolveEventArgs, Assembly>(nameof(OnTypeResolve), ResolveEventArgs.RealType, RealResolveEventHandler);
            resourceResolveReal = this.CreateEventDelegate<ResolveEventArgs, Assembly>(nameof(OnResourceResolve), ResolveEventArgs.RealType, RealResolveEventHandler);
#if DNX
            unhandledExceptionReal = this.CreateEventDelegate<UnhandledExceptionEventArgs>(nameof(OnUnhandledException), UnhandledExceptionEventArgs.RealType, RealUnhandledExceptionEventHandler);
#endif
        }

        /// <summary>
        ///     Gets the base directory that the assembly resolver uses to probe for assemblies.
        /// </summary>
        /// <value>
        ///     The base directory that the assembly resolver uses to probe for assemblies.
        /// </value>
        public string BaseDirectory => getBaseDirectory(appDomain);

        private Assembly OnAssemblyResolve(ResolveEventArgs args) => assemblyResolve?.Invoke(this, args);

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
                appDomain.AttachOrDetachEvent(assemblyResolve, assemblyResolveReal, assemblyResolveAdd);
                assemblyResolve = (ResolveEventHandler)Delegate.Combine(assemblyResolve, value);
            }
            remove
            {
                assemblyResolve = (ResolveEventHandler)Delegate.Remove(assemblyResolve, value);
                appDomain.AttachOrDetachEvent(assemblyResolve, assemblyResolveReal, assemblyResolveRemove);
            }
        }

        private Assembly OnTypeResolve(ResolveEventArgs args) => typeResolve?.Invoke(this, args);

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
                appDomain.AttachOrDetachEvent(typeResolve, typeResolveReal, typeResolveAdd);
                typeResolve = (ResolveEventHandler)Delegate.Combine(typeResolve, value);
            }
            remove
            {
                typeResolve = (ResolveEventHandler)Delegate.Remove(typeResolve, value);
                appDomain.AttachOrDetachEvent(typeResolve, typeResolveReal, typeResolveRemove);
            }
        }

        private Assembly OnResourceResolve(ResolveEventArgs args) => resourceResolve?.Invoke(this, args);

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
                appDomain.AttachOrDetachEvent(resourceResolve, resourceResolveReal, resourceResolveAdd);
                resourceResolve = (ResolveEventHandler)Delegate.Combine(resourceResolve, value);
            }
            remove
            {
                resourceResolve = (ResolveEventHandler)Delegate.Remove(resourceResolve, value);
                appDomain.AttachOrDetachEvent(resourceResolve, resourceResolveReal, resourceResolveRemove);
            }
        }

        private void OnUnhandledException(UnhandledExceptionEventArgs args) => unhandledException?.Invoke(this, args);

        /// <summary>
        ///     Occurs when an exception is not caught.
        /// </summary>
        /// <remarks>
        ///     This event provides notification of uncaught exceptions. It allows the application to log information about the exception before the system default handler reports the exception to the user and terminates the application. If sufficient information about the state of the application is available, other actions may be undertaken — such as saving program data for later recovery. Caution is advised, because program data can become corrupted when exceptions are not handled.
        /// </remarks>
        public event UnhandledExceptionEventHandler UnhandledException
        {
            add
            {
#if DNX
                appDomain.AttachOrDetachEvent(unhandledException, unhandledExceptionReal, unhandledExceptionAdd);
                unhandledException = (UnhandledExceptionEventHandler)Delegate.Combine(unhandledException, value);
#endif
            }
            remove
            {
#if DNX
                unhandledException = (UnhandledExceptionEventHandler)Delegate.Remove(unhandledException, value);
                appDomain.AttachOrDetachEvent(unhandledException, unhandledExceptionReal, unhandledExceptionRemove);
#endif
            }
        }
    }
}
