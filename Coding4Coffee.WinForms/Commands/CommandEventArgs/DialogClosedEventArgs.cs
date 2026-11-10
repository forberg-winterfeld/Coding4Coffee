namespace Coding4Coffee.WinForms.Commands.CommandEventArgs
{
    /// <summary>
    /// Event arguments for dialog closed events.
    /// Contains the dialog instance and its result.
    /// </summary>
    /// <typeparam name="TDialog">The type of the dialog.</typeparam>
    /// <remarks>
    /// Initializes a new instance of the <see cref="DialogClosedEventArgs{TDialog}"/> class.
    /// </remarks>
    /// <param name="dialog">The dialog instance that was closed.</param>
    /// <param name="result">The result of the dialog.</param>
    public class DialogClosedEventArgs<TDialog>(TDialog dialog, DialogResult result) : EventArgs
        where TDialog : CommonDialog
    {
        /// <summary>
        /// Gets the dialog instance that was closed.
        /// </summary>
        public TDialog Dialog { get; } = dialog;

        /// <summary>
        /// Gets the result of the dialog.
        /// </summary>
        public DialogResult DialogResult { get; } = result;
    }
}
