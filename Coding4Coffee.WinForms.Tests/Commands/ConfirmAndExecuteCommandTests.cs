using Coding4Coffee.Basics.Commands;
using Coding4Coffee.Basics.Localization;
using Coding4Coffee.WinForms.Commands;
using Coding4Coffee.WinForms.Commands.CommandData;
using System.Windows.Input;
using Xunit;

namespace Coding4Coffee.WinForms.Tests.Commands
{
    public class ConfirmAndExecuteCommandTests
    {
        private class TestCommand : ICommand
        {
            public bool CanExecuteValue { get; set; } = true;
            public bool WasExecuted { get; private set; }
            public object? LastParameter { get; private set; }

            public event EventHandler? CanExecuteChanged;

            public bool CanExecute(object? parameter) => CanExecuteValue;

            public void Execute(object? parameter)
            {
                WasExecuted = true;
                LastParameter = parameter;
            }

            public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }

        [Fact]
        public void Constructor_NullInnerCommand_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new ConfirmAndExecuteCommand(null!, () => true));
            Assert.Throws<ArgumentNullException>(() => new ConfirmAndExecuteCommand(null!, (ShowTaskDialogCommand)null!));
            Assert.Throws<ArgumentNullException>(() => new ConfirmAndExecuteCommand(null!, (ShowMessageBoxCommand)null!));
            Assert.Throws<ArgumentNullException>(() => new ConfirmAndExecuteCommand(null!, (LocalizableString)null!));
        }

        [Fact]
        public void Constructor_NullPredicate_ThrowsArgumentNullException()
        {
            var inner = new TestCommand();
            Assert.Throws<ArgumentNullException>(() => new ConfirmAndExecuteCommand(inner, (Func<bool>)null!));
        }

        [Fact]
        public void CanExecute_DelegatesToInnerCommand()
        {
            var inner = new TestCommand { CanExecuteValue = false };
            var command = new ConfirmAndExecuteCommand(inner, () => true);

            Assert.False(command.CanExecute(null));

            inner.CanExecuteValue = true;
            Assert.True(command.CanExecute(null));
        }

        [Fact]
        public void CanExecute_WhenDisposed_ReturnsFalse()
        {
            var inner = new TestCommand { CanExecuteValue = true };
            var command = new ConfirmAndExecuteCommand(inner, () => true);

            Assert.True(command.CanExecute(null));

            command.Dispose();

            Assert.False(command.CanExecute(null));
        }

        [Fact]
        public void InnerCommand_CanExecuteChanged_FiresWrapperCanExecuteChanged()
        {
            var inner = new TestCommand();
            var command = new ConfirmAndExecuteCommand(inner, () => true);
            bool eventFired = false;
            command.CanExecuteChanged += (_, _) => eventFired = true;

            inner.RaiseCanExecuteChanged();

            Assert.True(eventFired);
        }

        [Fact]
        public void Dispose_UnsubscribesFromInnerCommandCanExecuteChanged()
        {
            var inner = new TestCommand();
            var command = new ConfirmAndExecuteCommand(inner, () => true);
            bool eventFired = false;
            command.CanExecuteChanged += (_, _) => eventFired = true;

            command.Dispose();
            inner.RaiseCanExecuteChanged();

            Assert.False(eventFired);
        }

        [Fact]
        public void Execute_WhenConfirmed_ExecutesInnerCommand_AndRaisesConfirmed()
        {
            var inner = new TestCommand();
            var command = new ConfirmAndExecuteCommand(inner, () => true);

            DialogResult? confirmedResult = null;
            command.Confirmed += (_, res) => confirmedResult = res;

            command.Execute("TestData");

            Assert.True(inner.WasExecuted);
            Assert.Equal("TestData", inner.LastParameter);
            Assert.Equal(DialogResult.OK, command.LastDialogResult);
            Assert.Equal(DialogResult.OK, confirmedResult);
        }

        [Fact]
        public void Execute_WhenCancelled_DoesNotExecuteInnerCommand_AndRaisesCancelled()
        {
            var inner = new TestCommand();
            var command = new ConfirmAndExecuteCommand(inner, () => false);

            DialogResult? cancelledResult = null;
            command.Cancelled += (_, res) => cancelledResult = res;

            command.Execute("TestData");

            Assert.False(inner.WasExecuted);
            Assert.Null(inner.LastParameter);
            Assert.Equal(DialogResult.Cancel, command.LastDialogResult);
            Assert.Equal(DialogResult.Cancel, cancelledResult);
        }

        [Fact]
        public void Execute_WhenCannotExecute_DoesNothing()
        {
            var inner = new TestCommand { CanExecuteValue = false };
            bool confirmationEvaluated = false;
            var command = new ConfirmAndExecuteCommand(inner, () =>
            {
                confirmationEvaluated = true;
                return true;
            });

            command.Execute(null);

            Assert.False(confirmationEvaluated);
            Assert.False(inner.WasExecuted);
        }

        [Fact]
        public void Execute_WithCustomActionAndSpecificResult_WorksExpectedly()
        {
            var inner = new TestCommand();
            var command = new ConfirmAndExecuteCommand(
                inner,
                _ => DialogResult.Retry,
                result => result == DialogResult.Retry);

            command.Execute(null);

            Assert.True(inner.WasExecuted);
            Assert.Equal(DialogResult.Retry, command.LastDialogResult);
        }

        [Fact]
        public void Execute_WithExplicitOwner_PassesOwnerToConfirmationAction()
        {
            using var form = new Form();
            IWin32Window? receivedOwner = null;

            var inner = new TestCommand();
            var command = new ConfirmAndExecuteCommand(
                inner,
                owner =>
                {
                    receivedOwner = owner;
                    return DialogResult.OK;
                })
            {
                Owner = form
            };

            command.Execute("item");

            Assert.Same(form, receivedOwner);
            Assert.True(inner.WasExecuted);
            Assert.Equal("item", inner.LastParameter);
        }

        [Fact]
        public void Constructor_WithLocalizableString_CreatesValidTaskDialogCommand()
        {
            var inner = new TestCommand();
            var heading = new LocalizableString("Delete_Heading", "Eintrag löschen?");
            var text = new LocalizableString("Delete_Text", "Wirklich löschen?");

            var command = new ConfirmAndExecuteCommand(
                inner,
                heading,
                text,
                buttons: MessageBoxButtons.YesNo);

            Assert.NotNull(command.ConfirmationCommand);
            Assert.IsType<ShowTaskDialogCommand>(command.ConfirmationCommand);

            var taskDialogCommand = (ShowTaskDialogCommand)command.ConfirmationCommand;
            Assert.Same(heading, taskDialogCommand.SetupData.Heading);
            Assert.Same(text, taskDialogCommand.SetupData.Text);
            Assert.Equal(MessageBoxButtons.YesNo, taskDialogCommand.SetupData.Buttons);
        }

        [Fact]
        public void Constructor_WithString_CreatesValidTaskDialogCommand()
        {
            var inner = new TestCommand();
            var command = new ConfirmAndExecuteCommand(
                inner,
                heading: "Heading text",
                text: "Body text",
                buttons: MessageBoxButtons.OKCancel);

            Assert.NotNull(command.ConfirmationCommand);
            Assert.IsType<ShowTaskDialogCommand>(command.ConfirmationCommand);

            var taskDialogCommand = (ShowTaskDialogCommand)command.ConfirmationCommand;
            Assert.Equal("Heading text", taskDialogCommand.SetupData.Heading?.ToString());
            Assert.Equal(MessageBoxButtons.OKCancel, taskDialogCommand.SetupData.Buttons);
        }

        [Fact]
        public void Constructor_WithMessageBoxSetupData_CreatesValidMessageBoxCommand()
        {
            var inner = new TestCommand();
            var setup = new MessageBoxSetupData
            {
                Text = "Are you sure?",
                Caption = "Confirmation",
                Buttons = MessageBoxButtons.YesNo
            };

            var command = new ConfirmAndExecuteCommand(inner, setup);

            Assert.NotNull(command.ConfirmationCommand);
            Assert.IsType<ShowMessageBoxCommand>(command.ConfirmationCommand);
        }
    }
}
