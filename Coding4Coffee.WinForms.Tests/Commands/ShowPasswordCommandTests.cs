using System.Diagnostics;
using Coding4Coffee.WinForms.Commands;

namespace Coding4Coffee.WinForms.Tests.Commands
{
    public class ShowPasswordCommandTests
    {
        [Fact]
        public void Constructor_RejectsInvalidArguments()
        {
            using var textBox = new TextBox();
            Assert.Throws<ArgumentNullException>(() => new ShowPasswordCommand(null!));
            Assert.Throws<ArgumentOutOfRangeException>(() => new ShowPasswordCommand(textBox, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => new ShowPasswordCommand(textBox, -1));
            Assert.Throws<ArgumentOutOfRangeException>(() => new ShowPasswordCommand(textBox, 100, '\0'));
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public Task Execute_RevealsAndRemasksAfterDelay(bool useSystemPasswordChar) => RunOnUiThread(async form =>
        {
            using var textBox = new TextBox { Text = "secret", PasswordChar = '#', UseSystemPasswordChar = useSystemPasswordChar };
            form.Controls.Add(textBox);
            _ = textBox.Handle;
            var command = new ShowPasswordCommand(textBox, 300, '!');
            var finished = Completion(command);
            int completed = 0;
            Exception? failure = null;
            command.ExecutionCompleted += (_, _) => completed++;
            command.ExecutionFailed += (_, e) => failure = e.Exception;
            var elapsed = Stopwatch.StartNew();

            command.Execute(null);
            Assert.True(command.IsExecuting);
            Assert.False(command.CanExecute(null));
            Assert.Equal('\0', textBox.PasswordChar);
            Assert.False(textBox.UseSystemPasswordChar);
            Assert.Equal("secret", textBox.Text);
            command.Execute(null);
            await finished.WaitAsync(TimeSpan.FromSeconds(5));

            Assert.True(elapsed.Elapsed >= TimeSpan.FromMilliseconds(250));
            Assert.Null(failure);
            Assert.Equal(1, completed);
            Assert.False(command.IsExecuting);
            Assert.True(command.CanExecute(null));
            Assert.Equal('!', textBox.PasswordChar);
            Assert.False(textBox.UseSystemPasswordChar);
            Assert.Equal("secret", textBox.Text);
        });

        [Fact]
        public Task Cancel_RemasksImmediatelyAndReportsCancellation() => RunOnUiThread(async form =>
        {
            using var textBox = new TextBox { PasswordChar = '#' };
            form.Controls.Add(textBox);
            _ = textBox.Handle;
            var command = new ShowPasswordCommand(textBox);
            var finished = Completion(command);
            Exception? failure = null;
            command.ExecutionFailed += (_, e) => failure = e.Exception;

            command.Execute(null);
            Assert.Equal('\0', textBox.PasswordChar);
            command.Cancel();
            await finished.WaitAsync(TimeSpan.FromSeconds(5));

            Assert.IsType<OperationCanceledException>(failure);
            Assert.Equal('*', textBox.PasswordChar);
            Assert.False(textBox.UseSystemPasswordChar);
            Assert.False(command.IsExecuting);
        });

        [Fact]
        public Task Execute_AfterControlDisposal_CompletesWithoutAccessingDisposedControl() => RunOnUiThread(async form =>
        {
            var textBox = new TextBox { PasswordChar = '*' };
            form.Controls.Add(textBox);
            _ = textBox.Handle;
            var command = new ShowPasswordCommand(textBox, 50);
            var finished = Completion(command);
            Exception? failure = null;
            command.ExecutionFailed += (_, e) => failure = e.Exception;

            command.Execute(null);
            textBox.Dispose();
            await finished.WaitAsync(TimeSpan.FromSeconds(5));

            Assert.Null(failure);
            Assert.False(command.IsExecuting);
        });

        [Fact]
        public Task Execute_WithoutWinFormsContext_ReportsFailureWithoutRevealingPassword() => RunOnUiThread(form =>
        {
            using var textBox = new TextBox { PasswordChar = '#' };
            form.Controls.Add(textBox);
            _ = textBox.Handle;
            var command = new ShowPasswordCommand(textBox, 50);
            Exception? failure = null;
            command.ExecutionFailed += (_, e) => failure = e.Exception;
            var context = SynchronizationContext.Current;
            try
            {
                SynchronizationContext.SetSynchronizationContext(null);
                command.Execute(null);
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext(context);
            }

            Assert.IsType<InvalidOperationException>(failure);
            Assert.Equal('#', textBox.PasswordChar);
            Assert.False(command.IsExecuting);
            return Task.CompletedTask;
        });

        [Fact]
        public Task Execute_OnWrongThread_ReportsFailureWithoutRevealingPassword() => RunOnUiThread(async form =>
        {
            using var textBox = new TextBox { PasswordChar = '#' };
            form.Controls.Add(textBox);
            _ = textBox.Handle;
            var command = new ShowPasswordCommand(textBox, 50);
            Exception? failure = null;
            command.ExecutionFailed += (_, e) => failure = e.Exception;

            await Task.Run(() => command.Execute(null));

            Assert.IsType<InvalidOperationException>(failure);
            Assert.Equal('#', textBox.PasswordChar);
            Assert.False(command.IsExecuting);
        });

        [Fact]
        public void Execute_WithoutMessageLoop_ReportsFailureWithoutRevealingPassword()
        {
            using var textBox = new TextBox { PasswordChar = '#' };
            _ = textBox.Handle;
            var command = new ShowPasswordCommand(textBox, 50);
            Exception? failure = null;
            command.ExecutionFailed += (_, e) => failure = e.Exception;

            command.Execute(null);

            Assert.IsType<InvalidOperationException>(failure);
            Assert.Equal('#', textBox.PasswordChar);
            Assert.False(command.IsExecuting);
        }

        private static Task Completion(ShowPasswordCommand command)
        {
            var completed = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            command.CanExecuteChanged += (_, _) =>
            {
                if (!command.IsExecuting)
                    completed.TrySetResult();
            };
            return completed.Task;
        }

        private static async Task RunOnUiThread(Func<Form, Task> test)
        {
            var completion = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var thread = new Thread(() =>
            {
                using var form = new Form();
                form.Shown += async (_, _) =>
                {
                    try
                    {
                        await test(form);
                        completion.TrySetResult();
                    }
                    catch (Exception ex)
                    {
                        completion.TrySetException(ex);
                    }
                    finally
                    {
                        form.Close();
                    }
                };
                Application.Run(form);
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.IsBackground = true;
            thread.Start();
            await completion.Task.WaitAsync(TimeSpan.FromSeconds(10));
        }
    }
}
