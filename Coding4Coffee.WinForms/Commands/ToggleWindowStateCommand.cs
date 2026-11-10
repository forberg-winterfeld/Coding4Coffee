namespace Coding4Coffee.WinForms.Commands
{
    /// <summary>
    /// Command to toggle a <see cref="Form"/> between its maximized and normal window states.
    /// </summary>
    /// <remarks>
    /// If the form is currently in <see cref="FormWindowState.Maximized"/>, it is changed to <see cref="RestoredState"/> (defaults to <see cref="FormWindowState.Normal"/>).
    /// If the form is currently in <see cref="FormWindowState.Normal"/> or <see cref="FormWindowState.Minimized"/>, it is changed to <see cref="MaximizedState"/> (defaults to <see cref="FormWindowState.Maximized"/>).
    /// <see cref="FormWindowStateCommandBase.CanExecute(object?)"/> evaluates to <see langword="false"/> if <see cref="Form.MaximizeBox"/> is disabled.
    ///
    /// Typical usage in a custom title bar maximize/restore toggle button or header double-click action:
    /// <code>
    /// var toggleCommand = new ToggleWindowStateCommand(this);
    /// maximizeRestoreButton.BindCommand(toggleCommand);
    /// </code>
    /// </remarks>
    public class ToggleWindowStateCommand : FormWindowStateCommandBase
    {
        /// <summary>
        /// Gets the window state to apply when toggling from the restored state. Defaults to <see cref="FormWindowState.Maximized"/>.
        /// </summary>
        public FormWindowState MaximizedState { get; init; } = FormWindowState.Maximized;

        /// <summary>
        /// Gets the window state to apply when toggling from the maximized state. Defaults to <see cref="FormWindowState.Normal"/>.
        /// </summary>
        public FormWindowState RestoredState { get; init; } = FormWindowState.Normal;

        /// <summary>
        /// Initializes a new instance of the <see cref="ToggleWindowStateCommand"/> class bound to the specified form.
        /// </summary>
        /// <param name="form">The target form to toggle.</param>
        public ToggleWindowStateCommand(Form form)
            : base(form)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ToggleWindowStateCommand"/> class that resolves the form dynamically.
        /// </summary>
        /// <param name="formProvider">Optional delegate returning the target form.</param>
        public ToggleWindowStateCommand(Func<Form?>? formProvider = null)
            : base(formProvider)
        {
        }

        /// <inheritdoc />
        protected override bool CanExecuteForForm(Form form, object? _)
            => form.MaximizeBox;

        /// <inheritdoc />
        protected override void ExecuteForForm(Form form, object? _)
            => form.WindowState = form.WindowState == MaximizedState ? RestoredState : MaximizedState;
    }
}
