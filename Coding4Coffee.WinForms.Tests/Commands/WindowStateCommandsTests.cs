using Coding4Coffee.WinForms.Commands;
using Xunit;

namespace Coding4Coffee.WinForms.Tests.Commands
{
    public class WindowStateCommandsTests
    {
        [Fact]
        public void Constructor_NullForm_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new MinimizeFormCommand((Form)null!));
            Assert.Throws<ArgumentNullException>(() => new MaximizeFormCommand((Form)null!));
            Assert.Throws<ArgumentNullException>(() => new RestoreFormCommand((Form)null!));
            Assert.Throws<ArgumentNullException>(() => new ToggleWindowStateCommand((Form)null!));
        }

        [Fact]
        public void MinimizeFormCommand_CanExecute_And_Execute()
        {
            using var form = new Form { MinimizeBox = true, WindowState = FormWindowState.Normal };
            var command = new MinimizeFormCommand(form);

            Assert.True(command.CanExecute(null));

            command.Execute(null);

            Assert.Equal(FormWindowState.Minimized, form.WindowState);
            Assert.False(command.CanExecute(null));

            form.WindowState = FormWindowState.Normal;
            form.MinimizeBox = false;
            Assert.False(command.CanExecute(null));
        }

        [Fact]
        public void MaximizeFormCommand_CanExecute_And_Execute()
        {
            using var form = new Form { MaximizeBox = true, WindowState = FormWindowState.Normal };
            var command = new MaximizeFormCommand(form);

            Assert.True(command.CanExecute(null));

            command.Execute(null);

            Assert.Equal(FormWindowState.Maximized, form.WindowState);
            Assert.False(command.CanExecute(null));

            form.WindowState = FormWindowState.Normal;
            form.MaximizeBox = false;
            Assert.False(command.CanExecute(null));
        }

        [Fact]
        public void RestoreFormCommand_CanExecute_And_Execute()
        {
            using var form = new Form { WindowState = FormWindowState.Normal };
            var command = new RestoreFormCommand(form);

            Assert.False(command.CanExecute(null));

            form.WindowState = FormWindowState.Maximized;
            Assert.True(command.CanExecute(null));

            command.Execute(null);
            Assert.Equal(FormWindowState.Normal, form.WindowState);
            Assert.False(command.CanExecute(null));

            form.WindowState = FormWindowState.Minimized;
            Assert.True(command.CanExecute(null));

            command.Execute(null);
            Assert.Equal(FormWindowState.Normal, form.WindowState);
        }

        [Fact]
        public void ToggleWindowStateCommand_TogglesBetweenNormalAndMaximized()
        {
            using var form = new Form { MaximizeBox = true, WindowState = FormWindowState.Normal };
            var command = new ToggleWindowStateCommand(form);

            Assert.True(command.CanExecute(null));

            // Normal -> Maximized
            command.Execute(null);
            Assert.Equal(FormWindowState.Maximized, form.WindowState);

            // Maximized -> Normal
            command.Execute(null);
            Assert.Equal(FormWindowState.Normal, form.WindowState);

            // MaximizeBox disabled -> CanExecute false
            form.MaximizeBox = false;
            Assert.False(command.CanExecute(null));
        }

        [Fact]
        public void WindowStateCommand_ParameterDriven_ResolvesFormFromParameter()
        {
            using var form = new Form { MaximizeBox = true, WindowState = FormWindowState.Normal };
            var command = new MaximizeFormCommand();

            Assert.True(command.CanExecute(form));

            command.Execute(form);
            Assert.Equal(FormWindowState.Maximized, form.WindowState);
        }

        [Fact]
        public void WindowStateCommand_FormProvider_ResolvesForm()
        {
            using var form = new Form { MaximizeBox = true, WindowState = FormWindowState.Normal };
            var command = new MaximizeFormCommand(() => form);

            Assert.True(command.CanExecute(null));

            command.Execute(null);
            Assert.Equal(FormWindowState.Maximized, form.WindowState);
        }

        [Fact]
        public void WindowStateCommand_WhenDisposed_CanExecuteReturnsFalse()
        {
            using var form = new Form { MaximizeBox = true, WindowState = FormWindowState.Normal };
            var command = new MaximizeFormCommand(form);

            Assert.True(command.CanExecute(null));

            command.Dispose();

            Assert.False(command.CanExecute(null));
        }

        [Fact]
        public void WindowStateCommand_WhenFormDisposed_DisposesCommand()
        {
            var form = new Form { MaximizeBox = true, WindowState = FormWindowState.Normal };
            var command = new MaximizeFormCommand(form);

            Assert.True(command.CanExecute(null));

            form.Dispose();

            Assert.False(command.CanExecute(null));
        }

        [Fact]
        public void WindowStateCommand_FormResize_RaisesCanExecuteChanged()
        {
            using var form = new Form { MaximizeBox = true, WindowState = FormWindowState.Normal };
            var command = new MaximizeFormCommand(form);

            bool eventFired = false;
            command.CanExecuteChanged += (_, _) => eventFired = true;

            form.WindowState = FormWindowState.Maximized;

            Assert.True(eventFired);
        }

        [Fact]
        public void WindowStateCommand_RaiseCanExecuteChanged_FiresEvent()
        {
            using var form = new Form();
            var command = new MinimizeFormCommand(form);

            bool eventFired = false;
            command.CanExecuteChanged += (_, _) => eventFired = true;

            command.RaiseCanExecuteChanged();

            Assert.True(eventFired);
        }
    }
}
