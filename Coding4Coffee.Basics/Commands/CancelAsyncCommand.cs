using System.Windows.Input;

namespace Coding4Coffee.Basics.Commands
{
    /// <summary>
    /// Provides a command that requests cancellation of an <see cref="AsyncCommandBase"/>.
    /// </summary>
    /// <remarks>
    /// The command can execute while the target is executing. Cancellation is cooperative and the target
    /// remains executing until its operation finishes. Execution state notifications are forwarded on the
    /// thread that raises them. Dispose this command when it is no longer needed to unsubscribe from the target.
    /// </remarks>
    public sealed class CancelAsyncCommand : ICommand, IDisposable
    {
        private readonly AsyncCommandBase _target;
        private bool _disposed;

        /// <summary>
        /// Initializes a command that requests cancellation of the specified target.
        /// </summary>
        /// <param name="target">The asynchronous command to cancel.</param>
        /// <exception cref="ArgumentNullException"><paramref name="target"/> is <see langword="null"/>.</exception>
        public CancelAsyncCommand(AsyncCommandBase target)
        {
            ArgumentNullException.ThrowIfNull(target);
            _target = target;
            _target.CanExecuteChanged += Target_CanExecuteChanged;
        }

        /// <summary>
        /// Occurs when the target raises <see cref="AsyncCommandBase.CanExecuteChanged"/> or this command is disposed.
        /// </summary>
        public event EventHandler? CanExecuteChanged;

        /// <summary>
        /// Determines whether the target is executing and this command has not been disposed.
        /// </summary>
        /// <param name="_">The command parameter; ignored.</param>
        /// <returns><see langword="true"/> if cancellation can be requested; otherwise, <see langword="false"/>.</returns>
        public bool CanExecute(object? _) => !_disposed && _target.IsExecuting;

        /// <summary>
        /// Requests cancellation of the target if it is executing.
        /// </summary>
        /// <param name="_">The command parameter; ignored.</param>
        public void Execute(object? _)
        {
            if (CanExecute(null))
                _target.Cancel();
        }

        private void Target_CanExecuteChanged(object? _, EventArgs __)
            => CanExecuteChanged?.Invoke(this, EventArgs.Empty);

        /// <summary>
        /// Unsubscribes from the target and disables this command without cancelling the target.
        /// </summary>
        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;
            _target.CanExecuteChanged -= Target_CanExecuteChanged;
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
