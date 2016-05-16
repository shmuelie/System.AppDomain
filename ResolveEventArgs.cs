using System.Reflection;

namespace System
{
    /// <summary>
    ///     Provides data for loader resolution events, such as the <see cref="AppDomain.TypeResolve"/>, <see cref="AppDomain.ResourceResolve"/>, and <see cref="AppDomain.AssemblyResolve"/> events.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         When the loader cannot resolve an assembly reference and a handler has been provided for the appropriate loader resolution event, the event is raised and the <see cref="ResolveEventArgs"/> contains information about the item to be resolved.
    ///     </para>
    ///     <list type="bullet">
    ///         <item>The <see cref="Name"/> property contains the name of the item to be resolved.</item>
    ///         <item>Beginning with the .NET Framework 4, the <see cref="RequestingAssembly"/> property contains the assembly that requested an assembly that can provide the named item. For more information, see the <see cref="RequestingAssembly"/> property.</item>
    ///     </list>
    /// </remarks>
    /// <threadsafety static="true" instance="false"/>
    /// <seealso cref="AppDomain.TypeResolve"/>
    /// <seealso cref="AppDomain.ResourceResolve"/>
    /// <seealso cref="AppDomain.AssemblyResolve"/>
    public class ResolveEventArgs : EventArgs
    {
        internal static readonly Type RealType = typeof(string).GetTypeInfo().Assembly.GetType(typeof(ResolveEventArgs).FullName);
        private static readonly Func<object, string> getName;
        private static readonly Func<object, Assembly> getRequestingAssembly;

        static ResolveEventArgs()
        {
            getName = RealType.GetInstancePropertyFunction<string>(nameof(Name));
            getRequestingAssembly = RealType.GetInstancePropertyFunction<Assembly>(nameof(RequestingAssembly));
        }

        private readonly object resolveEventArgs;

        /// <summary>
        ///     Gets the name of the item to resolve.
        /// </summary>
        /// <value>
        ///     The name of the item to resolve.
        /// </value>
        /// <remarks>
        ///     For the <see cref="AppDomain.AssemblyResolve"/> event, Name is the assembly name before policy is applied.
        /// </remarks>
        public string Name
        {
            get
            {
                return getName(resolveEventArgs);
            }
        }

        /// <summary>
        ///     Gets the assembly whose dependency is being resolved.
        /// </summary>
        /// <value>
        ///     The assembly that requested the item specified by the <see cref="Name"/> property.
        /// </value>
        /// <remarks>
        ///     The assembly that is returned by this property is an assembly that was unable to resolve the item specified by the <see cref="Name"/> property, because the item did not exist in that assembly, in any of its loaded dependencies, or in any dependencies the loader could find through probing.
        /// </remarks>
        /// <seealso cref="AppDomain.TypeResolve"/>
        /// <seealso cref="AppDomain.ResourceResolve"/>
        /// <seealso cref="AppDomain.AssemblyResolve"/>
        public Assembly RequestingAssembly
        {
            get
            {
                return getRequestingAssembly(resolveEventArgs);
            }
        }

        /// <summary>
        ///     Creates a new instance of the <see cref="ResolveEventArgs"/> class from the internal version.
        /// </summary>
        /// <param name="resolveEventArgs">The internal version.</param>
        internal ResolveEventArgs(object resolveEventArgs)
        {
            resolveEventArgs.NotNull(nameof(resolveEventArgs));
            resolveEventArgs.InstanceOf(nameof(resolveEventArgs), RealType);
            this.resolveEventArgs = resolveEventArgs;
        }
    }
}
