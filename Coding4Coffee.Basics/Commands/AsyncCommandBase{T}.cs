namespace Coding4Coffee.Basics.Commands
{
    /// <summary>
    /// Provides an abstract base class for asynchronous commands that report execution progress and return a result.
    /// </summary>
    /// <typeparam name="TResult">The type of result returned by the command execution.</typeparam>
    /// <remarks>
    /// This class extends <see cref="AsyncCommandBase"/> to support:
    /// - Reporting progress during execution via <see cref="IProgress{T}"/>
    /// - Storing and accessing the last successful execution result
    /// - Proper cancellation and retry handling with result preservation
    /// </remarks>
    public abstract class AsyncCommandBase<TResult> : AsyncCommandBase
    {
        private TResult? _lastResult;
        private bool _hasResult;

        /// <summary>
        /// Occurs when progress is reported during command execution.
        /// </summary>
        public event EventHandler<AsyncCommandProgressEventArgs<TResult>>? ProgressReported;

        /// <summary>
        /// Gets the result from the last successful command execution.
        /// </summary>
        /// <remarks>
        /// This property is only valid if <see cref="HasResult"/> is <see langword="true"/>.
        /// If no successful execution has occurred, this property is undefined.
        /// </remarks>
        public TResult? LastResult
        {
            get => _lastResult;
            private set => _lastResult = value;
        }

        /// <summary>
        /// Gets a value indicating whether a successful command execution result is available.
        /// </summary>
        public bool HasResult
        {
            get => _hasResult;
            private set => _hasResult = value;
        }

        /// <summary>
        /// Defines the asynchronous logic to be executed when the command is invoked.
        /// Provides a progress reporter for progress tracking.
        /// </summary>
        /// <param name="parameter">Data used by the command.</param>
        /// <param name="progress">An instance of <see cref="IProgress{TResult}"/> that can be used to report progress.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to request cancellation of the operation.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation and its result.</returns>
        protected abstract Task<TResult> ExecuteAsync(object? parameter, IProgress<TResult> progress, CancellationToken cancellationToken);

        /// <summary>
        /// Implements the base async execution contract without result handling.
        /// Wraps the derived <see cref="ExecuteAsync(object?, IProgress{TResult}, CancellationToken)"/> call.
        /// </summary>
        /// <param name="parameter">Data used by the command.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to request cancellation of the operation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        protected sealed override async Task ExecuteAsync(object? parameter, CancellationToken cancellationToken)
        {
            var progress = new Progress<TResult>(ReportProgress);
            LastResult = await ExecuteAsync(parameter, progress, cancellationToken);
            HasResult = true;
        }

        /// <summary>
        /// Reports progress during command execution.
        /// </summary>
        /// <param name="progressValue">The progress value to report.</param>
        protected virtual void ReportProgress(TResult progressValue)
        {
            ProgressReported?.Invoke(this, new AsyncCommandProgressEventArgs<TResult>(progressValue, null));
        }

        /// <summary>
        /// Clears the cached result from the last execution.
        /// </summary>
        /// <remarks>
        /// Call this method to reset <see cref="HasResult"/> and prepare for a fresh execution.
        /// </remarks>
        public virtual void ClearResult()
        {
            LastResult = default;
            HasResult = false;
        }
    }
}
