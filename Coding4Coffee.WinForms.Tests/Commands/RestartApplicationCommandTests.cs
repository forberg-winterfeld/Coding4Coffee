using Coding4Coffee.WinForms.Commands;
using Xunit;

namespace Coding4Coffee.WinForms.Tests.Commands
{
    public class RestartApplicationCommandTests
    {
        [Fact]
        public void CanExecute_Default_ReturnsTrue()
        {
            var command = new RestartApplicationCommand();

            Assert.True(command.CanExecute(null));
        }

        [Fact]
        public void CanExecute_WithPredicate_EvaluatesPredicate()
        {
            bool allow = false;
            var command = new RestartApplicationCommand(_ => allow);

            Assert.False(command.CanExecute(null));

            allow = true;
            Assert.True(command.CanExecute(null));
        }

        [Fact]
        public void Execute_CallsRestartAction_AndFiresRestartingEvent()
        {
            bool restartActionCalled = false;
            bool restartingEventFired = false;

            var command = new RestartApplicationCommand(
                confirmRestart: null,
                canExecute: null,
                restartAction: () => restartActionCalled = true);

            command.Restarting += (_, _) => restartingEventFired = true;

            command.Execute(null);

            Assert.True(restartingEventFired);
            Assert.True(restartActionCalled);
        }

        [Fact]
        public void Execute_WhenConfirmRestartReturnsFalse_CancelsRestart()
        {
            bool restartActionCalled = false;
            bool restartingEventFired = false;

            var command = new RestartApplicationCommand(
                confirmRestart: () => false,
                canExecute: null,
                restartAction: () => restartActionCalled = true);

            command.Restarting += (_, _) => restartingEventFired = true;

            command.Execute(null);

            Assert.False(restartingEventFired);
            Assert.False(restartActionCalled);
        }

        [Fact]
        public void Execute_WhenConfirmRestartReturnsTrue_ProceedsWithRestart()
        {
            bool restartActionCalled = false;
            bool restartingEventFired = false;

            var command = new RestartApplicationCommand(
                confirmRestart: () => true,
                canExecute: null,
                restartAction: () => restartActionCalled = true);

            command.Restarting += (_, _) => restartingEventFired = true;

            command.Execute(null);

            Assert.True(restartingEventFired);
            Assert.True(restartActionCalled);
        }

        [Fact]
        public void Execute_WhenCannotExecute_DoesNothing()
        {
            bool restartActionCalled = false;

            var command = new RestartApplicationCommand(
                confirmRestart: null,
                canExecute: _ => false,
                restartAction: () => restartActionCalled = true);

            command.Execute(null);

            Assert.False(restartActionCalled);
        }

        [Fact]
        public void RaiseCanExecuteChanged_FiresEvent()
        {
            var command = new RestartApplicationCommand();
            bool eventFired = false;
            command.CanExecuteChanged += (_, _) => eventFired = true;

            command.RaiseCanExecuteChanged();

            Assert.True(eventFired);
        }
    }
}
