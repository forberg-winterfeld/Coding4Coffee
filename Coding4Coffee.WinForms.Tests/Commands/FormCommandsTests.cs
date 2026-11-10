using Coding4Coffee.WinForms.Commands;
using Xunit;

namespace Coding4Coffee.WinForms.Tests.Commands
{
    public class FormCommandsTests
    {
        [Fact]
        public void CloseFormCommand_NullForm_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new CloseFormCommand(null!));
        }

        [Fact]
        public void CloseFormCommand_CanExecute_TrueWhenFormActive_FalseWhenDisposed()
        {
            var form = new Form();
            var command = new CloseFormCommand(form);

            Assert.True(command.CanExecute(null));

            form.Dispose();

            Assert.False(command.CanExecute(null));
        }

        [Fact]
        public void CloseFormCommand_Execute_ClosesFormAndFiresCanExecuteChanged()
        {
            var form = new Form();
            var command = new CloseFormCommand(form);
            bool eventFired = false;
            command.CanExecuteChanged += (_, _) => eventFired = true;

            command.Execute(null);

            Assert.True(eventFired);
            Assert.True(form.IsDisposed);
            Assert.False(command.CanExecute(null));
        }

        [Fact]
        public void CloseFormCommand_Execute_WhenDisposed_DoesNotThrow()
        {
            var form = new Form();
            var command = new CloseFormCommand(form);
            form.Dispose();

            // Calling execute when disposed should silently return without error
            var exception = Record.Exception(() => command.Execute(null));
            Assert.Null(exception);
        }

        [Fact]
        public void OpenDialogCommand_NullFactory_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new OpenDialogCommand<OpenFileDialog>(null!, null, null));
        }

        [Fact]
        public void OpenDialogCommand_CanExecute_EvaluatesPredicate()
        {
            var command = new OpenDialogCommand<OpenFileDialog>(
                createDialog: () => new OpenFileDialog(),
                setupDialog: null,
                canExecute: param => param is string s && s == "yes");

            Assert.False(command.CanExecute(null));
            Assert.False(command.CanExecute("no"));
            Assert.True(command.CanExecute("yes"));
        }

        [Fact]
        public void OpenDialogCommand_RaiseCanExecuteChanged_FiresEvent()
        {
            var command = new OpenDialogCommand<OpenFileDialog>();
            bool eventFired = false;
            command.CanExecuteChanged += (_, _) => eventFired = true;

            command.RaiseCanExecuteChanged();

            Assert.True(eventFired);
        }

        [Fact]
        public void OpenFormCommand_NullFactory_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new OpenFormCommand<Form>((Func<Form>)null!));
        }

        [Fact]
        public void OpenFormCommand_NullCanExecute_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new OpenFormCommand<Form>((Func<bool>)null!));
        }

        [Fact]
        public void OpenFormCommand_CanExecute_EvaluatesPredicate()
        {
            bool allowed = false;
            var command = new OpenFormCommand<Form>(() => new Form(), () => allowed);

            Assert.False(command.CanExecute(null));

            allowed = true;
            Assert.True(command.CanExecute(null));
        }

        [Fact]
        public void OpenFormAsDialogCommand_NullFactory_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new OpenFormAsDialogCommand<Form>((Func<Form>)null!));
        }

        [Fact]
        public void OpenFormAsDialogCommand_CanExecute_EvaluatesPredicate()
        {
            var command = new OpenFormAsDialogCommand<Form>(
                createForm: () => new Form(),
                canExecute: param => param is int i && i == 1);

            Assert.False(command.CanExecute(null));
            Assert.False(command.CanExecute(0));
            Assert.True(command.CanExecute(1));
        }

        [Fact]
        public void OpenFormAsDialogCommand_RaiseCanExecuteChanged_FiresEvent()
        {
            var command = new OpenFormAsDialogCommand<Form>();
            bool eventFired = false;
            command.CanExecuteChanged += (_, _) => eventFired = true;

            command.RaiseCanExecuteChanged();

            Assert.True(eventFired);
        }
    }
}
