namespace System
{
    /// <summary>
    ///     Represents the method that will handle the event raised by an exception that is not handled by the application domain.
    /// </summary>
    /// <param name="sender">The source of the unhandled exception event. </param>
    /// <param name="e">An <see cref="UnhandledExceptionEventArgs"/> instance containing the event data.</param>
    /// <seealso cref="UnhandledExceptionEventArgs"/>
    /// <seealso cref="EventHandler"/>
    public delegate void UnhandledExceptionEventHandler(object sender, UnhandledExceptionEventArgs e);
}
