using System.Runtime.CompilerServices;

namespace System
{
    /// <summary>
    ///     Provides data for the event that is raised when there is an exception that is not handled in any application domain.
    /// </summary>
    /// <remarks>
    ///     <see cref="UnhandledExceptionEventArgs"/> provides access to the exception object and a flag indicating whether the common language runtime is terminating. The <see cref="UnhandledExceptionEventArgs"/> is one of the parameters passed into <see cref="UnhandledExceptionEventHandler"/> for the <see cref="AppDomain.UnhandledException"/> event.
    /// </remarks>
    /// <seealso cref="UnhandledExceptionEventHandler"/>
    /// <seealso cref="AppDomain.UnhandledException"/>
    /// <seealso cref="EventArgs" />
    /// <threadsafety static="true" instance="false"/>
    public class UnhandledExceptionEventArgs : EventArgs
    {
        internal static readonly Type RealType = typeof(UnhandledExceptionEventArgs).RealType();
        private static readonly Func<object, object> getExceptionObject;
        private static readonly Func<object, bool> getIsTerminating;

        static UnhandledExceptionEventArgs()
        {
            getExceptionObject = RealType.GetInstancePropertyFunction<object>(nameof(ExceptionObject));
            getIsTerminating = RealType.GetInstancePropertyFunction<bool>(nameof(IsTerminating));
        }

        private readonly object unhandledExceptionEventArgs;

        /// <summary>
        ///     Gets the unhandled exception object.
        /// </summary>
        /// <value>
        ///     The unhandled exception object.
        /// </value>
        /// <remarks>
        ///     <para>
        ///         This property returns an object of type <see cref="object"/> rather than one derived from <see cref="Exception"/>. Although the Common Language Specification requires that all exception types derive from <see cref="Exception"/>, it is possible for methods to throw exceptions with objects not derived from <see cref="Exception"/>. You can do the following to work with this exception:
        ///     </para>
        ///     <list type="bullet">
        ///         <item>Apply the <see cref="RuntimeCompatibilityAttribute"/> attribute with a <see cref="RuntimeCompatibilityAttribute.WrapNonExceptionThrows"/> value of <see langword="true"/> to the assembly that contains the event handler. This wraps all exceptions not derived from the <see cref="Exception"/> class in a RuntimeWrappedException object. You can then safely cast (in C#) or convert (in Visual Basic) the object returned by this property to an <see cref="Exception"/> object, and retrieve the original exception object from the RuntimeWrappedException.WrappedException property. Note that some compilers, such as the C# and Visual Basic compilers, automatically apply this attribute.</item>
        ///         <item>Cast the object returned by this property to an <see cref="Exception"/> object.</item>
        ///     </list>
        /// </remarks>
        /// <seealso cref="RuntimeCompatibilityAttribute"/>
        public object ExceptionObject => getExceptionObject(unhandledExceptionEventArgs);

        /// <summary>
        ///     Gets a value indicating whether the common language runtime is terminating.
        /// </summary>
        /// <value>
        ///     <see langword="true"/> if the runtime is terminating; otherwise, <see langword="false"/>.
        /// </value>
        /// <remarks>
        ///     Beginning with the .NET Framework version 2.0, this property returns <see langword="true"/> for most unhandled exceptions, unless an application compatibility flag is used to revert to the behavior of versions 1.0 and 1.1. The reason is that most unhandled exceptions in threads, including thread pool threads, are allowed to proceed naturally, which normally results in termination of the application.
        /// </remarks>
        public bool IsTerminating => getIsTerminating(unhandledExceptionEventArgs);

        internal UnhandledExceptionEventArgs(object unhandledExceptionEventArgs)
        {
            unhandledExceptionEventArgs.NotNull(nameof(unhandledExceptionEventArgs));
            unhandledExceptionEventArgs.InstanceOf(nameof(unhandledExceptionEventArgs), RealType);
            this.unhandledExceptionEventArgs = unhandledExceptionEventArgs;
        }
    }
}
