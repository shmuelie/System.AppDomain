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
        private static readonly Action<object, Delegate> assemblyResolveAdd;
        private static readonly Action<object, Delegate> assemblyResolveRemove;
        private static readonly Action<object, Delegate> typeResolveAdd;
        private static readonly Action<object, Delegate> typeResolveRemove;
        private static readonly Action<object, Delegate> resourceResolveAdd;
        private static readonly Action<object, Delegate> resourceResolveRemove;
        private static readonly Action<object, Delegate> unhandledExceptionAdd;
        private static readonly Action<object, Delegate> unhandledExceptionRemove;
        private static readonly Action<object, Delegate> processExitAdd;
        private static readonly Action<object, Delegate> processExitRemove;
        private static readonly Func<object, string> getBaseDirectory;
        private static readonly Func<object, string> getFriendlyName;
        private static readonly Func<object, bool> getIsHomogenous;
        private static readonly Func<object, bool> getIsFullyTrusted;
        private static readonly Func<object, Assembly[]> getAssembliesFunc;
        private static readonly Func<object, string, object> getGetDataFunc;

        static AppDomain()
        {
#pragma warning disable HeapAnalyzerExplicitNewObjectRule // Explicit new reference type allocation
            currentDomain = new Lazy<AppDomain>(CreateCurrentDomain);
#pragma warning restore HeapAnalyzerExplicitNewObjectRule // Explicit new reference type allocation
            RealType.GetEventMethods(nameof(AssemblyResolve), out assemblyResolveAdd, out assemblyResolveRemove);
            RealType.GetEventMethods(nameof(TypeResolve), out typeResolveAdd, out typeResolveRemove);
            RealType.GetEventMethods(nameof(ResourceResolve), out resourceResolveAdd, out resourceResolveRemove);
            RealType.GetEventMethods(nameof(UnhandledException), out unhandledExceptionAdd, out unhandledExceptionRemove);
            RealType.GetEventMethods(nameof(ProcessExit), out processExitAdd, out processExitRemove);
            getBaseDirectory = RealType.GetInstancePropertyFunction<string>(nameof(BaseDirectory));
            getFriendlyName = RealType.GetInstancePropertyFunction<string>(nameof(FriendlyName));
            getIsHomogenous = RealType.GetInstancePropertyFunction<bool>(nameof(IsHomogenous));
            getIsFullyTrusted = RealType.GetInstancePropertyFunction<bool>(nameof(IsFullyTrusted));
            getAssembliesFunc = RealType.GetInstanceFunctionFunction<Assembly[]>(nameof(GetAssemblies));
            getGetDataFunc = RealType.GetInstanceFunctionFunction<string, object>(nameof(GetData));
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
        private readonly Delegate processExitReal;
        private EventHandler processExit;

        internal AppDomain(object appDomain)
        {
            appDomain.NotNull(nameof(appDomain));
            appDomain.InstanceOf(nameof(appDomain), RealType);
            this.appDomain = appDomain;
            if (assemblyResolveAdd != null)
            {
                assemblyResolveReal = this.CreateEventDelegate<ResolveEventArgs, Assembly>(nameof(OnAssemblyResolve), ResolveEventArgs.RealType, RealResolveEventHandler);
            }
            if (typeResolveAdd != null)
            {
                typeResolveReal = this.CreateEventDelegate<ResolveEventArgs, Assembly>(nameof(OnTypeResolve), ResolveEventArgs.RealType, RealResolveEventHandler);
            }
            if (resourceResolveAdd != null)
            {
                resourceResolveReal = this.CreateEventDelegate<ResolveEventArgs, Assembly>(nameof(OnResourceResolve), ResolveEventArgs.RealType, RealResolveEventHandler);
            }
            if (unhandledExceptionAdd != null)
            {
                unhandledExceptionReal = this.CreateEventDelegate<UnhandledExceptionEventArgs>(nameof(OnUnhandledException), UnhandledExceptionEventArgs.RealType, RealUnhandledExceptionEventHandler);
            }
            if (processExitAdd != null)
            {
                processExitReal = this.CreateEventDelegate<EventArgs>(nameof(OnProcessExit), typeof(EventArgs), typeof(EventHandler));
            }
        }

        /// <summary>
        ///     Gets the base directory that the assembly resolver uses to probe for assemblies.
        /// </summary>
        /// <value>
        ///     The base directory that the assembly resolver uses to probe for assemblies.
        /// </value>
        public string BaseDirectory => getBaseDirectory(appDomain);

        /// <summary>
        ///     Gets the friendly name of this application domain.
        /// </summary>
        /// <value>
        ///     The friendly name of this application domain.
        /// </value>
        /// <remarks>
        ///     The friendly name of the default application domain is the file name of the process executable. For example, if the executable used to start the process is "c:\MyAppDirectory\MyAssembly.exe", the friendly name of the default application domain is "MyAssembly.exe".
        /// </remarks>
        public string FriendlyName => getFriendlyName?.Invoke(appDomain) ?? string.Empty;

        /// <summary>
        ///     Gets a value that indicates whether the current application domain has a set of permissions that is granted to all assemblies that are loaded into the application domain.
        /// </summary>
        /// <value>
        ///     <see langword="true"/> if the current application domain has a homogenous set of permissions; otherwise, <see langword="false"/>.
        /// </value>
        public bool IsHomogenous => getIsHomogenous?.Invoke(appDomain) ?? false;

        /// <summary>
        ///     Gets a value that indicates whether assemblies that are loaded into the current application domain execute with full trust.
        /// </summary>
        /// <value>
        ///     <see langword="true"/> if assemblies that are loaded into the current application domain execute with full trust; otherwise, <see langword="false"/>.
        /// </value>
        public bool IsFullyTrusted => getIsFullyTrusted?.Invoke(appDomain) ?? false;

        /// <summary>
        ///     Gets the assemblies that have been loaded into the execution context of this application domain.
        /// </summary>
        /// <returns>An array of assemblies in this application domain.</returns>
        public Assembly[] GetAssemblies() => getAssembliesFunc?.Invoke(appDomain) ?? Array.Empty<Assembly>();

        /// <summary>
        ///     Gets the value stored in the current application domain for the specified name.
        /// </summary>
        /// <param name="name">The name of a predefined application domain property, or the name of an application domain property you have defined.</param>
        /// <returns>The value of the <paramref name="name"/> property, or <see langword="null"/> if the property does not exist.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="name"/> is <see langword="null"/>.</exception>
        public object GetData(string name)
        {
            name.NotNull(nameof(name));
            return getGetDataFunc?.Invoke(appDomain, name);
        }

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
                if (assemblyResolveAdd != null)
                {
                    appDomain.AttachOrDetachEvent(assemblyResolve, assemblyResolveReal, assemblyResolveAdd);
                    assemblyResolve = (ResolveEventHandler)Delegate.Combine(assemblyResolve, value);
                }
            }
            remove
            {
                if (assemblyResolveAdd != null)
                {
                    assemblyResolve = (ResolveEventHandler)Delegate.Remove(assemblyResolve, value);
                    appDomain.AttachOrDetachEvent(assemblyResolve, assemblyResolveReal, assemblyResolveRemove);
                }
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
                if (typeResolveAdd != null)
                {
                    appDomain.AttachOrDetachEvent(typeResolve, typeResolveReal, typeResolveAdd);
                    typeResolve = (ResolveEventHandler)Delegate.Combine(typeResolve, value);
                }
            }
            remove
            {
                if (typeResolveAdd != null)
                {
                    typeResolve = (ResolveEventHandler)Delegate.Remove(typeResolve, value);
                    appDomain.AttachOrDetachEvent(typeResolve, typeResolveReal, typeResolveRemove);
                }
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
                if (resourceResolveAdd != null)
                {
                    appDomain.AttachOrDetachEvent(resourceResolve, resourceResolveReal, resourceResolveAdd);
                    resourceResolve = (ResolveEventHandler)Delegate.Combine(resourceResolve, value);
                }
            }
            remove
            {
                if (resourceResolveAdd != null)
                {
                    resourceResolve = (ResolveEventHandler)Delegate.Remove(resourceResolve, value);
                    appDomain.AttachOrDetachEvent(resourceResolve, resourceResolveReal, resourceResolveRemove);
                }
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
                if (unhandledExceptionAdd != null)
                {
                    appDomain.AttachOrDetachEvent(unhandledException, unhandledExceptionReal, unhandledExceptionAdd);
                    unhandledException = (UnhandledExceptionEventHandler)Delegate.Combine(unhandledException, value);
                }
            }
            remove
            {
                if (unhandledExceptionAdd != null)
                {
                    unhandledException = (UnhandledExceptionEventHandler)Delegate.Remove(unhandledException, value);
                    appDomain.AttachOrDetachEvent(unhandledException, unhandledExceptionReal, unhandledExceptionRemove);
                }
            }
        }

        private void OnProcessExit(EventArgs args) => processExit?.Invoke(this, args);

        /// <summary>
        ///     Occurs when the default application domain's parent process exits.
        /// </summary>
        /// <remarks>
        ///     The <see cref="EventHandler"/> for this event can perform termination activities, such as closing files, releasing storage and so on, before the process ends.
        /// </remarks>
        public event EventHandler ProcessExit
        {
            add
            {
                appDomain.AttachOrDetachEvent(processExit, processExitReal, processExitAdd);
                processExit = (EventHandler)Delegate.Combine(processExit, value);
            }
            remove
            {
                processExit = (EventHandler)Delegate.Remove(processExit, value);
                appDomain.AttachOrDetachEvent(processExit, processExitReal, processExitRemove);
            }
        }
    }
}
