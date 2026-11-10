 using System.Windows.Input;

namespace Coding4Coffee.WinForms.Commands
{
    /// <summary>
    /// Command that closes a specified Windows Form.
    /// </summary>
    /// <remarks>
    /// Monitors the target <see cref="Form"/>'s lifecycle. If the form is already disposed
    /// or in the process of disposing, <see cref="CanExecute(object?)"/> evaluates to <see langword="false"/>
    /// and execution is ignored without throwing an exception.
    /// </remarks>
    public sealed class CloseFormCommand : ICommand
    {
        private readonly Form _form;

        /// <summary>
        /// Event that is raised when the command's ability to execute changes.
        /// </summary>
        public event EventHandler? CanExecuteChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="CloseFormCommand"/> class for the specified form.
        /// </summary>
        /// <param name="form">The form to close.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="form"/> is <see langword="null"/>.</exception>
        public CloseFormCommand(Form form)
        {
            ArgumentNullException.ThrowIfNull(form);
            _form = form;
        }

        /// <summary>
        /// Returns whether the command can execute.
        /// </summary>
        /// <param name="_">The command parameter (ignored).</param>
        /// <returns><see langword="true"/> if the form is not disposed or disposing; otherwise, <see langword="false"/>.</returns>
        public bool CanExecute(object? _)
            => !_form.IsDisposed && !_form.Disposing;

        /// <summary>
        /// Executes the command, closing the associated form.
        /// </summary>
        /// <remarks>
        /// Verifies <see cref="CanExecute(object?)"/> prior to calling <see cref="Form.Close()"/>.
        /// If the form is disposed or disposing, no action is taken.
        /// Raises <see cref="CanExecuteChanged"/> after closing.
        /// </remarks>
        /// <param name="_">The command parameter (ignored).</param>
        public void Execute(object? _)
        {
            if (!CanExecute(_))
                return;

            _form.Close();
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
