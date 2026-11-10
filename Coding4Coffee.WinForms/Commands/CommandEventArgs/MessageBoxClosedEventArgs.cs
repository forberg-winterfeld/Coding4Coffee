namespace Coding4Coffee.WinForms.Commands.CommandEventArgs
{
    /// <summary>
    /// Event arguments for message box closed events.
    /// Contains the resulting <see cref="DialogResult"/>.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="MessageBoxClosedEventArgs"/> class.
    /// </remarks>
    /// <param name="result">The dialog result returned by the message box.</param>
    public class MessageBoxClosedEventArgs(DialogResult result) : EventArgs
    {
        /// <summary>
        /// Gets the dialog result of the message box.
        /// </summary>
        public DialogResult DialogResult { get; } = result;
    }
}
