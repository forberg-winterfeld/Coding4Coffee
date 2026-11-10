using Coding4Coffee.Basics.Commands;
using System.Windows.Input;

namespace Coding4Coffee.WinForms.Commands
{
    /// <summary>
    /// WinForms implementation of application restart command that cleanly restarts the application via <see cref="Application.Restart()"/>.
    /// </summary>
    /// <remarks>
    /// Initiates a clean shutdown of the current application instance and starts a new process instance.
    /// Useful after changing application culture, themes, database configurations, or installing updates.
    ///
    /// Typical usage in a settings form or menu:
    /// <code>
    /// var restartCommand = new RestartApplicationCommand();
    /// restartButton.BindCommand(restartCommand);
    /// </code>
    ///
    /// Typical usage with restart confirmation:
    /// <code>
    /// var restartCommand = new RestartApplicationCommand(confirmRestart: () =>
    ///     MessageBox.Show("Möchten Sie das Programm jetzt neu starten?", "Neu starten", MessageBoxButtons.YesNo) == DialogResult.Yes);
    /// </code>
    /// </remarks>
    public class RestartApplicationCommand : IRestartApplicationCommand
    {
        private readonly Func<bool>? _confirmRestart;
        private readonly Func<object?, bool>? _canExecute;
        private readonly Action? _restartAction;

        /// <summary>
        /// Event that is raised when the command's ability to execute changes.
        /// </summary>
        public event EventHandler? CanExecuteChanged;

        /// <summary>
        /// Event that is raised immediately before the application restart is initiated.
        /// </summary>
        public event EventHandler? Restarting;

        /// <summary>
        /// Initializes a new parameterless instance of the <see cref="RestartApplicationCommand"/> class.
        /// </summary>
        public RestartApplicationCommand()
            : this(confirmRestart: null, canExecute: null, restartAction: null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RestartApplicationCommand"/> class with a custom <paramref name="canExecute"/> predicate.
        /// </summary>
        /// <param name="canExecute">Optional predicate to determine whether the command can execute.</param>
        public RestartApplicationCommand(Func<object?, bool>? canExecute)
            : this(confirmRestart: null, canExecute: canExecute, restartAction: null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RestartApplicationCommand"/> class with an optional confirmation callback and predicate.
        /// </summary>
        /// <param name="confirmRestart">Optional callback returning <see langword="true"/> to allow restart or <see langword="false"/> to cancel.</param>
        /// <param name="canExecute">Optional predicate to determine whether the command can execute.</param>
        public RestartApplicationCommand(Func<bool>? confirmRestart, Func<object?, bool>? canExecute = null)
            : this(confirmRestart, canExecute, restartAction: null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RestartApplicationCommand"/> class with full configuration for testing.
        /// </summary>
        /// <param name="confirmRestart">Optional callback returning <see langword="true"/> to allow restart or <see langword="false"/> to cancel.</param>
        /// <param name="canExecute">Optional predicate to determine whether the command can execute.</param>
        /// <param name="restartAction">Custom action to invoke for application restart (defaults to <see cref="Application.Restart()"/>).</param>
        internal RestartApplicationCommand(Func<bool>? confirmRestart, Func<object?, bool>? canExecute, Action? restartAction)
        {
            _confirmRestart = confirmRestart;
            _canExecute = canExecute;
            _restartAction = restartAction;
        }

        /// <summary>
        /// Determines whether the command can execute in its current state.
        /// </summary>
        /// <param name="parameter">The command parameter (evaluated by the optional <c>canExecute</c> predicate).</param>
        /// <returns><see langword="true"/> if the command can execute; otherwise, <see langword="false"/>.</returns>
        public virtual bool CanExecute(object? parameter)
            => _canExecute?.Invoke(parameter) ?? true;

        /// <summary>
        /// Executes the command, prompting for optional confirmation and restarting the application.
        /// </summary>
        /// <param name="parameter">The command parameter (ignored unless used by configured predicates).</param>
        public virtual void Execute(object? parameter)
        {
            if (!CanExecute(parameter))
                return;

            if (_confirmRestart != null && !_confirmRestart())
                return;

            OnRestarting();

            if (_restartAction != null)
                _restartAction();
            else
                Application.Restart();
        }

        /// <summary>
        /// Raises the <see cref="CanExecuteChanged"/> event.
        /// </summary>
        public void RaiseCanExecuteChanged()
            => CanExecuteChanged?.Invoke(this, EventArgs.Empty);

        /// <summary>
        /// Raises the <see cref="Restarting"/> event.
        /// </summary>
        protected virtual void OnRestarting()
            => Restarting?.Invoke(this, EventArgs.Empty);
    }
}
