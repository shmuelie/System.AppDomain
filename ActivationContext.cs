using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace System
{
    /// <summary>
    ///     Identifies the activation context for the current application. This class cannot be inherited.
    /// </summary>
    /// <seealso cref="IDisposable" />
    /// <threadsafety static="true" instance="false"/>
    public sealed class ActivationContext : IDisposable
    {
        internal static readonly Type RealType = typeof(ActivationContext).RealType();
        private static readonly Func<object, byte[]> getApplicationManifestBytes = RealType.GetInstancePropertyFunction<byte[]>(nameof(ApplicationManifestBytes));
        private static readonly Func<object, byte[]> getDeploymentManifestBytes = RealType.GetInstancePropertyFunction<byte[]>(nameof(DeploymentManifestBytes));
        private static readonly Func<object, ContextForm> getForm = RealType.GetInstancePropertyFunction<ContextForm>(nameof(Form));
        private static readonly Func<object, object> getIdentity = RealType.GetInstancePropertyFunction<object>(nameof(Identity));
        private static readonly Func<ApplicationIdentity, ActivationContext> createPartialActivationContext = RealType.GetStaticFunctionFunction<ApplicationIdentity, ActivationContext>(nameof(CreatePartialActivationContext));

        private readonly object activationContext;

        internal ActivationContext(object activationContext)
        {
            activationContext.NotNull(nameof(activationContext));
            activationContext.InstanceOf(nameof(activationContext), RealType);
            this.activationContext = activationContext;
        }

        /// <summary>
        ///     Gets the ClickOnce application manifest for the current application.
        /// </summary>
        /// <value>
        ///     A byte array that contains the ClickOnce application manifest for the application that is associated with this <see cref="ActivationContext"/>.
        /// </value>
        public byte[] ApplicationManifestBytes => getApplicationManifestBytes(activationContext);

        /// <summary>
        ///     Gets the ClickOnce deployment manifest for the current application.
        /// </summary>
        /// <value>
        ///     A byte array that contains the ClickOnce deployment manifest for the application that is associated with this <see cref="ActivationContext"/>.
        /// </value>
        public byte[] DeploymentManifestBytes => getDeploymentManifestBytes(activationContext);

        /// <summary>
        ///     Gets the form, or store context, for the current application.
        /// </summary>
        /// <value>
        ///     One of the enumeration values.
        /// </value>
        public ContextForm Form => getForm(activationContext);

        /// <summary>
        ///     Gets the application identity for the current application.
        /// </summary>
        /// <value>
        ///     An <see cref="ApplicationIdentity"/> object that identifies the current application.
        /// </value>
        public ApplicationIdentity Identity => ApplicationIdentity.CacheNew(getIdentity(activationContext));

        /// <summary>
        ///     Initializes a new instance of the <see cref="ActivationContext"/> class using the specified application identity.
        /// </summary>
        /// <param name="identity">An object that identifies an application.</param>
        /// <returns>An object with the specified application identity.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="identity"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">No deployment or application identity is specified in <paramref name="identity"/>.</exception>
        public static ActivationContext CreatePartialActivationContext(ApplicationIdentity identity)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ActivationContext"/> class using the specified application identity and array of manifest paths.
        /// </summary>
        /// <param name="identity">An object that identifies an application.</param>
        /// <param name="manifestPaths">A string array of manifest paths for the application.</param>
        /// <returns>An object with the specified application identity and array of manifest paths.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="identity"/> is <see langword="null"/> - or - <paramref name="manifestPaths"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">No deployment or application identity is specified in <paramref name="identity"/> -or - <paramref name="identity"/> does not match the identity in the manifests - or - <paramref name="identity"/> does not have the same number of components as the manifest paths.</exception>
        public static ActivationContext CreatePartialActivationContext(ApplicationIdentity identity, string[] manifestPaths)
        {
        }

        /// <summary>
        ///     Indicates the context for a manifest-activated application.
        /// </summary>
        public enum ContextForm
        {
            /// <summary>
            ///     The application is not in the ClickOnce store.
            /// </summary>
            Loose,
            /// <summary>
            ///     The application is contained in the ClickOnce store.
            /// </summary>
            StoreBounded
        }

        /// <summary>
        ///     Releases all resources used by the <see cref="ActivationContext"/>.
        /// </summary>
        public void Dispose()
        {
        }

        /// <summary>
        ///     Enables an <see cref="ActivationContext"/> object to attempt to free resources and perform other cleanup operations before the <see cref="ActivationContext"/> is reclaimed by garbage collection
        /// </summary>
        ~ActivationContext()
        {

        }
    }
}
