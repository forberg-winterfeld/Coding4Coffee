namespace Coding4Coffee.WinForms.Commands
{
    /// <summary>
    /// Command to restore a minimized or maximized <see cref="Form"/> to its normal window state.
    /// </summary>
    /// <remarks>
    /// Sets <see cref="Form.WindowState"/> to <see cref="FormWindowState.Normal"/>.
    /// <see cref="FormWindowStateCommandBase.CanExecute(object?)"/> evaluates to <see langword="false"/>
    /// if the form is already in the <see cref="FormWindowState.Normal"/> state.
    ///
    /// Typical usage in a custom title bar or system tray menu:
    /// <code>
    /// var restoreCommand = new RestoreFormCommand(this);
    /// restoreMenuItem.BindCommand(restoreCommand);
    /// </code>
    /// </remarks>
    public class RestoreFormCommand : FormWindowStateCommandBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RestoreFormCommand"/> class bound to the specified form.
        /// </summary>
        /// <param name="form">The target form to restore.</param>
        public RestoreFormCommand(Form form)
            : base(form)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RestoreFormCommand"/> class that resolves the form dynamically.
        /// </summary>
        /// <param name="formProvider">Optional delegate returning the target form.</param>
        public RestoreFormCommand(Func<Form?>? formProvider = null)
            : base(formProvider)
        {
        }

        /// <inheritdoc />
        protected override bool CanExecuteForForm(Form form, object? _)
            => form.WindowState != FormWindowState.Normal;

        /// <inheritdoc />
        protected override void ExecuteForForm(Form form, object? _)
            => form.WindowState = FormWindowState.Normal;
    }
}
