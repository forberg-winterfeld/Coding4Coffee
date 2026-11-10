namespace Coding4Coffee.WinForms.Commands
{
    /// <summary>
    /// Command to minimize a <see cref="Form"/> to the taskbar.
    /// </summary>
    /// <remarks>
    /// Sets <see cref="Form.WindowState"/> to <see cref="FormWindowState.Minimized"/>.
    /// <see cref="FormWindowStateCommandBase.CanExecute(object?)"/> evaluates to <see langword="false"/>
    /// if the form is already minimized or if <see cref="Form.MinimizeBox"/> is disabled.
    ///
    /// Typical usage in a custom title bar:
    /// <code>
    /// var minimizeCommand = new MinimizeFormCommand(this);
    /// minimizeButton.BindCommand(minimizeCommand);
    /// </code>
    /// </remarks>
    public class MinimizeFormCommand : FormWindowStateCommandBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MinimizeFormCommand"/> class bound to the specified form.
        /// </summary>
        /// <param name="form">The target form to minimize.</param>
        public MinimizeFormCommand(Form form)
            : base(form)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MinimizeFormCommand"/> class that resolves the form dynamically.
        /// </summary>
        /// <param name="formProvider">Optional delegate returning the target form.</param>
        public MinimizeFormCommand(Func<Form?>? formProvider = null)
            : base(formProvider)
        {
        }

        /// <inheritdoc />
        protected override bool CanExecuteForForm(Form form, object? _)
            => form.MinimizeBox && form.WindowState != FormWindowState.Minimized;

        /// <inheritdoc />
        protected override void ExecuteForForm(Form form, object? _)
            => form.WindowState = FormWindowState.Minimized;
    }
}
