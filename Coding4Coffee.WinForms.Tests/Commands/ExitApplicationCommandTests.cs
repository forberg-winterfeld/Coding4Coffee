using Coding4Coffee.WinForms.Commands;
using Xunit;

namespace Coding4Coffee.WinForms.Tests.Commands
{
    public class ExitApplicationCommandTests
    {
        [Fact]
        public void CanExecute_Default_ReturnsTrue()
        {
            var command = new ExitApplicationCommand();

            Assert.True(command.CanExecute(null));
        }

        [Fact]
        public void CanExecute_WithPredicate_EvaluatesPredicate()
        {
            bool allow = false;
            var command = new ExitApplicationCommand(_ => allow);

            Assert.False(command.CanExecute(null));

            allow = true;
            Assert.True(command.CanExecute(null));
        }

        [Fact]
        public void Execute_CallsExitAction_AndFiresExitingEvent()
        {
            bool exitActionCalled = false;
            bool exitingEventFired = false;

            var command = new ExitApplicationCommand(
                confirmExit: null,
                canExecute: null,
                exitAction: () => exitActionCalled = true);

            command.Exiting += (_, _) => exitingEventFired = true;

            command.Execute(null);

            Assert.True(exitingEventFired);
            Assert.True(exitActionCalled);
        }

        [Fact]
        public void Execute_WhenConfirmExitReturnsFalse_CancelsExit()
        {
            bool exitActionCalled = false;
            bool exitingEventFired = false;

            var command = new ExitApplicationCommand(
                confirmExit: () => false,
                canExecute: null,
                exitAction: () => exitActionCalled = true);

            command.Exiting += (_, _) => exitingEventFired = true;

            command.Execute(null);

            Assert.False(exitingEventFired);
            Assert.False(exitActionCalled);
        }

        [Fact]
        public void Execute_WhenConfirmExitReturnsTrue_ProceedsWithExit()
        {
            bool exitActionCalled = false;
            bool exitingEventFired = false;

            var command = new ExitApplicationCommand(
                confirmExit: () => true,
                canExecute: null,
                exitAction: () => exitActionCalled = true);

            command.Exiting += (_, _) => exitingEventFired = true;

            command.Execute(null);

            Assert.True(exitingEventFired);
            Assert.True(exitActionCalled);
        }

        [Fact]
        public void Execute_WhenCannotExecute_DoesNothing()
        {
            bool exitActionCalled = false;

            var command = new ExitApplicationCommand(
                confirmExit: null,
                canExecute: _ => false,
                exitAction: () => exitActionCalled = true);

            command.Execute(null);

            Assert.False(exitActionCalled);
        }

        [Fact]
        public void RaiseCanExecuteChanged_FiresEvent()
        {
            var command = new ExitApplicationCommand();
            bool eventFired = false;
            command.CanExecuteChanged += (_, _) => eventFired = true;

            command.RaiseCanExecuteChanged();

            Assert.True(eventFired);
        }
    }
}
