namespace Coding4Coffee.WinForms.Commands.CommandEventArgs
{
    /// <summary>
    /// Event arguments for form closed events.
    /// Contains the form instance and its dialog result.
    /// </summary>
    /// <typeparam name="TForm">The type of the form.</typeparam>
    /// <remarks>
    /// Initializes a new instance of the <see cref="FormClosedEventArgs{TForm}"/> class.
    /// </remarks>
    /// <param name="form">The form instance that was closed.</param>
    /// <param name="result">The dialog result of the form.</param>
    public class FormClosedEventArgs<TForm>(TForm form, DialogResult result) : EventArgs
        where TForm : Form
    {
        /// <summary>
        /// Gets the form instance that was closed.
        /// </summary>
        public TForm Form { get; } = form;

        /// <summary>
        /// Gets the dialog result of the form.
        /// </summary>
        public DialogResult DialogResult { get; } = result;
    }
}
