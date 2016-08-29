namespace System
{
    /// <summary>
    ///     Provides the ability to uniquely identify a manifest-activated application. This class cannot be inherited.
    /// </summary>
    /// <remarks>
    ///     The ApplicationIdentity class is used in the activation of manifest-based applications.
    /// </remarks>
    /// <threadsafety static="true" instance="false"/>
    public sealed class ApplicationIdentity
    {
        internal static readonly Type RealType = typeof(ApplicationIdentity).RealType();
        private static readonly Func<string, object> externalConstructor = RealType.CreateConstructor<string>();
        private static readonly Func<object, string> getCodeBase = RealType.GetInstancePropertyFunction<string>(nameof(CodeBase));
        private static readonly Func<object, string> getFullName = RealType.GetInstancePropertyFunction<string>(nameof(FullName));
        private static readonly Func<object, string> toString = RealType.GetInstanceFunctionFunction<string>(nameof(ToString));

        private readonly object applicationIdentity;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ApplicationIdentity"/> class.
        /// </summary>
        /// <param name="applicationIdentityFullName">The full name of the application.</param>
        /// <exception cref="ArgumentNullException"><paramref name="applicationIdentityFullName"/> is <see langword="null"/>.</exception>
        public ApplicationIdentity(string applicationIdentityFullName)
        {
            applicationIdentityFullName.NotNull(nameof(applicationIdentityFullName));
            applicationIdentity = externalConstructor(applicationIdentityFullName);
        }

        internal ApplicationIdentity(object applicationIdentity)
        {
            applicationIdentity.NotNull(nameof(applicationIdentity));
            applicationIdentity.InstanceOf(nameof(applicationIdentity), RealType);
            this.applicationIdentity = applicationIdentity;
        }

        /// <summary>
        ///     Gets the location of the deployment manifest as a URL.
        /// </summary>
        /// <value>
        ///     The URL of the deployment manifest.
        /// </value>
        public string CodeBase => getCodeBase(applicationIdentity);

        /// <summary>
        ///     Gets the full name of the application.
        /// </summary>
        /// <value>
        ///     The full name of the application, also known as the display name.
        /// </value>
        public string FullName => getFullName(applicationIdentity);

        /// <summary>
        ///     Returns the full name of the manifest-activated application.
        /// </summary>
        /// <returns>
        ///     The full name of the manifest-activated application.
        /// </returns>
        public override string ToString() => toString(applicationIdentity);
    }
}
