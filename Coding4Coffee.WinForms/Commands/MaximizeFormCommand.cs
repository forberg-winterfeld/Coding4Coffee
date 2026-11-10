namespace Coding4Coffee.WinForms.Commands
{
    /// <summary>
    /// Command to maximize a <see cref="Form"/> to fill the screen.
    /// </summary>
    /// <remarks>
    /// Sets <see cref="Form.WindowState"/> to <see cref="FormWindowState.Maximized"/>.
    /// <see cref="FormWindowStateCommandBase.CanExecute(object?)"/> evaluates to <see langword="false"/>
    /// if the form is already maximized or if <see cref="Form.MaximizeBox"/> is disabled.
    ///
    /// Typical usage in a custom title bar:
    /// <code>
    /// var maximizeCommand = new MaximizeFormCommand(this);
    /// maximizeButton.BindCommand(maximizeCommand);
    /// </code>
    /// </remarks>
    public class MaximizeFormCommand : FormWindowStateCommandBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MaximizeFormCommand"/> class bound to the specified form.
        /// </summary>
        /// <param name="form">The target form to maximize.</param>
        public MaximizeFormCommand(Form form)
            : base(form)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MaximizeFormCommand"/> class that resolves the form dynamically.
        /// </summary>
        /// <param name="formProvider">Optional delegate returning the target form.</param>
        public MaximizeFormCommand(Func<Form?>? formProvider = null)
            : base(formProvider)
        {
        }

        /// <inheritdoc />
        protected override bool CanExecuteForForm(Form form, object? _)
            => form.MaximizeBox && form.WindowState != FormWindowState.Maximized;

        /// <inheritdoc />
        protected override void ExecuteForForm(Form form, object? _)
            => form.WindowState = FormWindowState.Maximized;
    }
}
