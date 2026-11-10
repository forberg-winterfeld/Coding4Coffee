using Coding4Coffee.Basics.Commands;
using System.Windows.Input;

namespace Coding4Coffee.WinForms.Commands
{
    /// <summary>
    /// WinForms implementation of application exit command that cleanly terminates the application via <see cref="Application.Exit()"/>.
    /// </summary>
    /// <remarks>
    /// Unlike <see cref="CloseFormCommand"/>, which operates on an individual <see cref="Form"/> instance,
    /// <see cref="ExitApplicationCommand"/> informs all message loops to stop and closes all open windows,
    /// allowing each form's <see cref="Form.FormClosing"/> event to run.
    ///
    /// Typical usage in a main menu or tray icon:
    /// <code>
    /// var exitCommand = new ExitApplicationCommand();
    /// exitMenuItem.BindCommand(exitCommand);
    /// </code>
    ///
    /// Typical usage with exit confirmation:
    /// <code>
    /// var exitCommand = new ExitApplicationCommand(confirmExit: () =>
    ///     MessageBox.Show("Möchten Sie das Programm wirklich beenden?", "Beenden", MessageBoxButtons.YesNo) == DialogResult.Yes);
    /// </code>
    /// </remarks>
    public class ExitApplicationCommand : IExitApplicationCommand
    {
        private readonly Func<bool>? _confirmExit;
        private readonly Func<object?, bool>? _canExecute;
        private readonly Action? _exitAction;

        /// <summary>
        /// Event that is raised when the command's ability to execute changes.
        /// </summary>
        public event EventHandler? CanExecuteChanged;

        /// <summary>
        /// Event that is raised immediately before the application exit is initiated.
        /// </summary>
        public event EventHandler? Exiting;

        /// <summary>
        /// Initializes a new parameterless instance of the <see cref="ExitApplicationCommand"/> class.
        /// </summary>
        public ExitApplicationCommand()
            : this(confirmExit: null, canExecute: null, exitAction: null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExitApplicationCommand"/> class with a custom <paramref name="canExecute"/> predicate.
        /// </summary>
        /// <param name="canExecute">Optional predicate to determine whether the command can execute.</param>
        public ExitApplicationCommand(Func<object?, bool>? canExecute)
            : this(confirmExit: null, canExecute: canExecute, exitAction: null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExitApplicationCommand"/> class with an optional confirmation callback and predicate.
        /// </summary>
        /// <param name="confirmExit">Optional callback returning <see langword="true"/> to allow exit or <see langword="false"/> to cancel.</param>
        /// <param name="canExecute">Optional predicate to determine whether the command can execute.</param>
        public ExitApplicationCommand(Func<bool>? confirmExit, Func<object?, bool>? canExecute = null)
            : this(confirmExit, canExecute, exitAction: null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExitApplicationCommand"/> class with full configuration for testing.
        /// </summary>
        /// <param name="confirmExit">Optional callback returning <see langword="true"/> to allow exit or <see langword="false"/> to cancel.</param>
        /// <param name="canExecute">Optional predicate to determine whether the command can execute.</param>
        /// <param name="exitAction">Custom action to invoke for application exit (defaults to <see cref="Application.Exit()"/>).</param>
        internal ExitApplicationCommand(Func<bool>? confirmExit, Func<object?, bool>? canExecute, Action? exitAction)
        {
            _confirmExit = confirmExit;
            _canExecute = canExecute;
            _exitAction = exitAction;
        }

        /// <summary>
        /// Determines whether the command can execute in its current state.
        /// </summary>
        /// <param name="parameter">The command parameter (evaluated by the optional <c>canExecute</c> predicate).</param>
        /// <returns><see langword="true"/> if the command can execute; otherwise, <see langword="false"/>.</returns>
        public virtual bool CanExecute(object? parameter)
            => _canExecute?.Invoke(parameter) ?? true;

        /// <summary>
        /// Executes the command, prompting for optional confirmation and shutting down the application.
        /// </summary>
        /// <param name="parameter">The command parameter (ignored unless used by configured predicates).</param>
        public virtual void Execute(object? parameter)
        {
            if (!CanExecute(parameter))
                return;

            if (_confirmExit != null && !_confirmExit())
                return;

            OnExiting();

            if (_exitAction != null)
                _exitAction();
            else
                Application.Exit();
        }

        /// <summary>
        /// Raises the <see cref="CanExecuteChanged"/> event.
        /// </summary>
        public void RaiseCanExecuteChanged()
            => CanExecuteChanged?.Invoke(this, EventArgs.Empty);

        /// <summary>
        /// Raises the <see cref="Exiting"/> event.
        /// </summary>
        protected virtual void OnExiting()
            => Exiting?.Invoke(this, EventArgs.Empty);
    }
}
