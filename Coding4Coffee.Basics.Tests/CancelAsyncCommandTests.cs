using Coding4Coffee.Basics.Commands;

namespace Coding4Coffee.Basics.Tests
{
    public class CancelAsyncCommandTests
    {
        private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(5);

        private sealed class ControlledCommand : AsyncCommandBase
        {
            public TaskCompletionSource<CancellationToken> Started { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
            public TaskCompletionSource Release { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
            public int Attempts { get; private set; }
            public CancellationToken LastToken { get; private set; }
            public bool ObserveCancellation { get; set; } = true;

            protected override async Task ExecuteAsync(object? _, CancellationToken cancellationToken)
            {
                Attempts++;
                LastToken = cancellationToken;
                Started.TrySetResult(cancellationToken);
                if (ObserveCancellation)
                    await Release.Task.WaitAsync(cancellationToken);
                else
                    await Release.Task;
            }
        }

        private sealed class RetryCommand : AsyncCommandBase
        {
            public TaskCompletionSource RetryScheduled { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
            public int Attempts { get; private set; }

            protected override TimeSpan RetryDelay
            {
                get
                {
                    RetryScheduled.TrySetResult();
                    return TimeSpan.FromSeconds(30);
                }
            }

            protected override Task ExecuteAsync(object? _, CancellationToken __)
            {
                Attempts++;
                throw new InvalidOperationException("Attempt failed");
            }
        }

        private sealed class ResultCommand : AsyncCommandBase<int>
        {
            public TaskCompletionSource Started { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

            protected override async Task<int> ExecuteAsync(object? _, IProgress<int> __, CancellationToken cancellationToken)
            {
                Started.TrySetResult();
                await Task.Delay(System.Threading.Timeout.InfiniteTimeSpan, cancellationToken);
                return 1;
            }
        }

        [Fact]
        public void Constructor_RejectsNullTarget()
        {
            Assert.Throws<ArgumentNullException>(() => new CancelAsyncCommand(null!));
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Execute_DuringStartupNotification_CancelsBeforeOperation(bool cancelDuringCanExecuteChanged)
        {
            var target = new ControlledCommand();
            using var cancel = new CancelAsyncCommand(target);
            Exception? failure = null;
            target.ExecutionFailed += (_, e) => failure = e.Exception;
            if (cancelDuringCanExecuteChanged)
                cancel.CanExecuteChanged += (_, _) => { if (cancel.CanExecute(null)) cancel.Execute(null); };
            else
                target.ExecutionStarted += (_, _) => cancel.Execute(null);

            target.Execute(null);

            Assert.IsType<OperationCanceledException>(failure);
            Assert.Equal(0, target.Attempts);
            Assert.False(target.IsExecuting);
            Assert.False(cancel.CanExecute(null));
        }

        [Fact]
        public async Task Execute_CancelsRunningCommand_AndForwardsStateChanges()
        {
            var target = new ControlledCommand();
            using var cancel = new CancelAsyncCommand(target);
            var states = new List<bool>();
            var finished = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            Exception? failure = null;
            target.ExecutionFailed += (_, e) => failure = e.Exception;
            cancel.CanExecuteChanged += (sender, _) =>
            {
                Assert.Same(cancel, sender);
                states.Add(cancel.CanExecute(null));
                if (!target.IsExecuting)
                    finished.TrySetResult();
            };

            Assert.False(cancel.CanExecute(null));
            cancel.Execute(null);
            target.Execute(null);
            CancellationToken token = await target.Started.Task.WaitAsync(Timeout);
            Assert.True(cancel.CanExecute(null));
            cancel.Execute(null);
            await finished.Task.WaitAsync(Timeout);

            Assert.True(token.IsCancellationRequested);
            Assert.IsType<OperationCanceledException>(failure);
            Assert.Equal(new[] { true, false }, states);
            Assert.Equal(1, target.Attempts);
        }

        [Fact]
        public async Task Execute_DuringRetryDelay_CancelsWithoutAnotherAttempt()
        {
            var target = new RetryCommand { MaxRetries = 1 };
            using var cancel = new CancelAsyncCommand(target);
            var finished = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            Exception? failure = null;
            target.ExecutionFailed += (_, e) => failure = e.Exception;
            target.CanExecuteChanged += (_, _) => { if (!target.IsExecuting) finished.TrySetResult(); };

            target.Execute(null);
            await target.RetryScheduled.Task.WaitAsync(Timeout);
            cancel.Execute(null);
            await finished.Task.WaitAsync(Timeout);

            Assert.IsType<OperationCanceledException>(failure);
            Assert.Equal(1, target.Attempts);
            Assert.False(cancel.CanExecute(null));
        }

        [Fact]
        public async Task Execute_WhenOperationIgnoresCancellation_RemainsEnabledUntilCompletion()
        {
            var target = new ControlledCommand { ObserveCancellation = false };
            using var cancel = new CancelAsyncCommand(target);
            var finished = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            target.CanExecuteChanged += (_, _) => { if (!target.IsExecuting) finished.TrySetResult(); };
            bool completed = false;
            target.ExecutionCompleted += (_, _) => completed = true;

            target.Execute(null);
            CancellationToken token = await target.Started.Task.WaitAsync(Timeout);
            cancel.Execute(null);
            Assert.True(token.IsCancellationRequested);
            Assert.True(cancel.CanExecute(null));
            target.Release.TrySetResult();
            await finished.Task.WaitAsync(Timeout);

            Assert.True(completed);
            Assert.False(cancel.CanExecute(null));
        }

        [Fact]
        public async Task Dispose_DisablesCommandAndStopsForwardingWithoutCancellingTarget()
        {
            var target = new ControlledCommand();
            var cancel = new CancelAsyncCommand(target);
            int notifications = 0;
            cancel.CanExecuteChanged += (_, _) => notifications++;
            var finished = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            target.CanExecuteChanged += (_, _) => { if (!target.IsExecuting) finished.TrySetResult(); };

            target.Execute(null);
            CancellationToken token = await target.Started.Task.WaitAsync(Timeout);
            cancel.Dispose();
            cancel.Dispose();
            cancel.Execute(null);
            Assert.False(cancel.CanExecute(null));
            Assert.False(token.IsCancellationRequested);
            target.Release.TrySetResult();
            await finished.Task.WaitAsync(Timeout);

            Assert.Equal(2, notifications);
        }

        [Fact]
        public async Task Execute_AfterCancellation_UsesFreshTokenForNextExecution()
        {
            var target = new ControlledCommand();
            using var cancel = new CancelAsyncCommand(target);
            var finished = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            target.CanExecuteChanged += (_, _) => { if (!target.IsExecuting) finished.TrySetResult(); };
            target.Execute(null);
            CancellationToken firstToken = await target.Started.Task.WaitAsync(Timeout);
            cancel.Execute(null);
            await finished.Task.WaitAsync(Timeout);

            target.Release.TrySetResult();
            target.Execute(null);
            Assert.Equal(2, target.Attempts);
            Assert.True(firstToken.IsCancellationRequested);
            Assert.False(target.IsExecuting);
            Assert.True(target.LastToken.CanBeCanceled);
            Assert.NotEqual(firstToken, target.LastToken);
            Assert.False(target.LastToken.IsCancellationRequested);
        }

        [Fact]
        public async Task Execute_CancelsGenericCommand()
        {
            var target = new ResultCommand();
            using var cancel = new CancelAsyncCommand(target);
            var finished = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            Exception? failure = null;
            target.ExecutionFailed += (_, e) => failure = e.Exception;
            target.CanExecuteChanged += (_, _) => { if (!target.IsExecuting) finished.TrySetResult(); };

            target.Execute(null);
            await target.Started.Task.WaitAsync(Timeout);
            cancel.Execute(null);
            await finished.Task.WaitAsync(Timeout);

            Assert.IsType<OperationCanceledException>(failure);
            Assert.False(target.HasResult);
        }
    }
}
