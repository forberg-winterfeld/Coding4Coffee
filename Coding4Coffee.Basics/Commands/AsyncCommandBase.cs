using System.Windows.Input;

namespace Coding4Coffee.Basics.Commands
{
    /// <summary>
    /// Provides an abstract base class for asynchronous <see cref="ICommand"/> implementations 
    /// that prevent concurrent execution (re-entrancy protection) using atomic operations 
    /// and support comprehensive async command scenarios including cancellation, error handling, 
    /// progress reporting, retry logic, and lifecycle events.
    /// </summary>
    public abstract class AsyncCommandBase : ICommand
    {
        private int _isExecuting; // 0 = false (not executing), 1 = true (executing)
        private readonly Lock _executionLock = new();
        private CancellationTokenSource? _currentCancellationTokenSource;
        private int _maxRetries = 0;

        /// <summary>
        /// Occurs when changes occur that affect whether the command should execute.
        /// </summary>
        public event EventHandler? CanExecuteChanged;

        /// <summary>
        /// Occurs when the command execution starts.
        /// </summary>
        public event EventHandler? ExecutionStarted;

        /// <summary>
        /// Occurs when the command execution completes successfully.
        /// </summary>
        public event EventHandler? ExecutionCompleted;

        /// <summary>
        /// Occurs when the command execution fails with an exception.
        /// </summary>
        public event EventHandler<AsyncCommandErrorEventArgs>? ExecutionFailed;

        /// <summary>
        /// Gets a value indicating whether the command is currently executing.
        /// </summary>
        public bool IsExecuting => Volatile.Read(ref _isExecuting) == 1;

        /// <summary>
        /// Gets or sets the maximum number of retries for failed executions.
        /// </summary>
        /// <remarks>
        /// Default is 0 (no retries). Cancelled operations are not retried.
        /// The setter only allows changes when the command is not currently executing.
        /// If an attempt is made to set this property while executing, an <see cref="InvalidOperationException"/> is thrown.
        /// </remarks>
        /// <exception cref="InvalidOperationException">Thrown if attempting to set this property while the command is executing.</exception>
        public int MaxRetries
        {
            get => _maxRetries;
            set
            {
                if (IsExecuting)
                {
                    throw new InvalidOperationException("Cannot change MaxRetries while the command is executing.");
                }
                _maxRetries = value;
            }
        }

        /// <summary>
        /// Gets the initial delay before the first retry attempt.
        /// </summary>
        /// <remarks>
        /// Each subsequent retry doubles the delay (exponential backoff with base 2).
        /// Default is 100 milliseconds.
        /// </remarks>
        protected virtual TimeSpan RetryDelay => TimeSpan.FromMilliseconds(100);

        /// <summary>
        /// Defines the method that determines whether the command can execute in its current state.
        /// Combines the execution lock check with custom conditions defined in derived classes.
        /// </summary>
        /// <param name="parameter">Data used by the command. This parameter is passed to <see cref="CanExecuteOverride(object?)"/>.</param>
        /// <returns>
        /// <see langword="true"/> if this command can be executed; otherwise, <see langword="false"/>.
        /// </returns>
        public virtual bool CanExecute(object? parameter)
        {
            // First check if already running, then evaluate custom conditions
            return Volatile.Read(ref _isExecuting) == 0 && CanExecuteOverride(parameter);
        }

        /// <summary>
        /// Defines additional conditions that determine whether the command can execute.
        /// </summary>
        /// <param name="parameter">Data used by the command.</param>
        /// <returns>
        /// <see langword="true"/> if the command can execute; otherwise, <see langword="false"/>.
        /// </returns>
        protected virtual bool CanExecuteOverride(object? parameter) => true;

        /// <summary>
        /// Defines the method to be called when the command is invoked.
        /// Manages the atomic execution lock, retry logic, cancellation, and async workflow.
        /// </summary>
        /// <remarks>
        /// A single cancellation token is used for all attempts and retry delays. Cancellation requested before an
        /// attempt prevents that attempt from starting; a running attempt must observe the token to stop early.
        /// Cancellation is reported through <see cref="ExecutionFailed"/> and is not retried.
        /// Exceptions from startup notifications are not retried or reported through <see cref="ExecutionFailed"/>.
        /// The execution state is reset and <see cref="CanExecuteChanged"/> is raised again before the exception
        /// is propagated by the <see langword="async"/> <see langword="void"/> method. An exception from this final
        /// notification can replace the original exception.
        /// </remarks>
        /// <param name="parameter">Data used by the command. If the command does not require data to be passed, this object can be set to <see langword="null"/>.</param>
        public async void Execute(object? parameter)
        {
            if (!CanExecute(parameter))
                return;

            CancellationTokenSource cancellationSource;
            lock (_executionLock)
            {
                if (Interlocked.CompareExchange(ref _isExecuting, 1, 0) != 0)
                    return;
                cancellationSource = new CancellationTokenSource();
                _currentCancellationTokenSource = cancellationSource;
            }

            try
            {
                RaiseCanExecuteChanged();
                OnExecutionStarted();

                int retryCount = 0;
                while (true)
                {
                    try
                    {
                        cancellationSource.Token.ThrowIfCancellationRequested();
                        await ExecuteAsync(parameter, cancellationSource.Token);
                        OnExecutionCompleted();
                        return;
                    }
                    catch (OperationCanceledException)
                    {
                        // Cancelled operations are not retried
                        OnError(new OperationCanceledException("Command execution was cancelled."), parameter);
                        return;
                    }
                    catch (Exception ex)
                    {
                        retryCount++;
                        if (retryCount > MaxRetries)
                        {
                            OnError(ex, parameter);
                            return;
                        }

                        int delayMs = (int)RetryDelay.TotalMilliseconds * (1 << (retryCount - 1));
                        try
                        {
                            await Task.Delay(delayMs, cancellationSource.Token);
                        }
                        catch (OperationCanceledException)
                        {
                            OnError(new OperationCanceledException("Command execution was cancelled."), parameter);
                            return;
                        }
                    }
                }
            }
            finally
            {
                lock (_executionLock)
                {
                    _currentCancellationTokenSource = null;
                    Interlocked.Exchange(ref _isExecuting, 0);
                }
                RaiseCanExecuteChanged();
            }
        }

        /// <summary>
        /// Defines the asynchronous logic to be executed when the command is invoked.
        /// </summary>
        /// <param name="parameter">Data used by the command.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to request cancellation of the operation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        protected abstract Task ExecuteAsync(object? parameter, CancellationToken cancellationToken);

        /// <summary>
        /// Called when the command execution completes successfully.
        /// </summary>
        /// <remarks>
        /// Override this method to perform cleanup or post-execution logic.
        /// The default implementation raises the <see cref="ExecutionCompleted"/> event.
        /// </remarks>
        protected virtual void OnExecutionCompleted()
        {
            RaiseExecutionCompleted();
        }

        /// <summary>
        /// Called when the command execution fails with an exception.
        /// </summary>
        /// <param name="exception">The exception that occurred during execution.</param>
        /// <param name="parameter">The parameter passed to the command.</param>
        /// <remarks>
        /// Override this method to implement custom error handling logic.
        /// The default implementation raises the <see cref="ExecutionFailed"/> event.
        /// </remarks>
        protected virtual void OnError(Exception exception, object? parameter)
        {
            ExecutionFailed?.Invoke(this, new AsyncCommandErrorEventArgs(exception, parameter));
        }

        /// <summary>
        /// Called when the command execution starts.
        /// </summary>
        /// <remarks>
        /// Override this method to perform startup logic or initialization.
        /// The default implementation raises the <see cref="ExecutionStarted"/> event.
        /// </remarks>
        protected virtual void OnExecutionStarted()
        {
            ExecutionStarted?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Raises the <see cref="CanExecuteChanged"/> event to notify listeners that the execution state has changed.
        /// </summary>
        public virtual void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Raises the <see cref="ExecutionCompleted"/> event.
        /// </summary>
        protected virtual void RaiseExecutionCompleted()
        {
            ExecutionCompleted?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Requests cancellation of the currently executing command operation.
        /// </summary>
        /// <remarks>
        /// Cancellation is cooperative: the operation must observe its token. Cancellation also interrupts retry delays.
        /// If no operation is currently running, this method has no effect. Cancellation callbacks run synchronously.
        /// </remarks>
        public virtual void Cancel()
        {
            CancellationTokenSource? cancellationSource;
            lock (_executionLock)
                cancellationSource = _currentCancellationTokenSource;
            cancellationSource?.Cancel();
        }
    }
}