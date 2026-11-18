using System.Windows.Input;
using Coding4Coffee.WinForms.Commands.Controls;
using Coding4Coffee.WinForms.Localization.Controls;

namespace Coding4Coffee.WinForms.Tests.Commands
{
    public class LocalizableButtonWithCommandSupportTests
    {
        [Fact]
        public void CommandParameter_ChangesThroughButtonBase_RefreshEnabledWithCurrentParameter()
        {
            using var button = new LocalizableButtonWithCommandSupport();
            var command = new TestCommand(parameter => parameter is string text && text == "allowed");
            button.Command = command;
            Assert.False(button.Enabled);

            ButtonBase nativeButton = button;
            nativeButton.CommandParameter = "allowed";
            Assert.True(button.Enabled);
            Assert.Equal("allowed", command.LastCheckedParameter);

            nativeButton.CommandParameter = "blocked";
            Assert.False(button.Enabled);
            Assert.Equal("blocked", command.LastCheckedParameter);

            nativeButton.CommandParameter = null;
            Assert.False(button.Enabled);
            Assert.Null(command.LastCheckedParameter);
        }

        [Fact]
        public void ParameterChanged_WithoutCommand_PreservesEnabledState()
        {
            using var button = new LocalizableButtonWithCommandSupport { Enabled = false };
            button.CommandParameter = "selection";
            Assert.False(button.Enabled);
        }

        [Fact]
        public void EqualParameter_DoesNotTriggerAnotherCheck()
        {
            using var button = new LocalizableButtonWithCommandSupport();
            var command = new TestCommand(_ => true);
            button.BindCommand(command, "selection");
            int checks = command.CheckCount;

            button.CommandParameter = "selection";

            Assert.Equal(checks, command.CheckCount);
        }

        [Fact]
        public void CanExecuteChanged_RefreshesStateForSameParameter()
        {
            using var button = new LocalizableButtonWithCommandSupport();
            bool allowed = false;
            var command = new TestCommand(_ => allowed);
            var selection = new object();
            button.BindCommand(command, selection);
            Assert.False(button.Enabled);

            allowed = true;
            command.NotifyCanExecuteChanged();

            Assert.True(button.Enabled);
            Assert.Same(selection, command.LastCheckedParameter);
        }

        [Fact]
        public void RemovingCommand_RestoresOriginalEnabledState()
        {
            using var button = new LocalizableButtonWithCommandSupport();
            var command = new TestCommand(_ => false);
            button.Command = command;
            Assert.False(button.Enabled);

            button.Command = null;
            button.CommandParameter = "selection";
            command.NotifyCanExecuteChanged();

            Assert.True(button.Enabled);
        }

        [Fact]
        public void ParameterChangedHandler_CanReplaceCommandBeforeRefresh()
        {
            using var button = new LocalizableButtonWithCommandSupport();
            var first = new TestCommand(_ => false);
            var replacement = new TestCommand(parameter => Equals(parameter, "selection"));
            button.Command = first;
            button.CommandParameterChanged += (_, _) => button.Command = replacement;

            button.CommandParameter = "selection";

            Assert.Same(replacement, button.Command);
            Assert.True(button.Enabled);
            Assert.Equal("selection", replacement.LastCheckedParameter);
        }

        [Fact]
        public void BindCommand_UsesNativeCommandAndClickExecutesOnce()
        {
            using var button = new TestButton();
            var command = new TestCommand(parameter => Equals(parameter, "selection"));

            LocalizableButtonWithCommandSupport result = button.BindCommand(command, "selection");
            button.RaiseClick();

            Assert.Same(button, result);
            Assert.IsAssignableFrom<LocalizableButton>(button);
            Assert.Same(command, button.Command);
            Assert.Equal(1, command.ExecuteCount);
            Assert.Equal("selection", command.LastExecutedParameter);
        }

        private sealed class TestButton : LocalizableButtonWithCommandSupport
        {
            public void RaiseClick() => OnClick(EventArgs.Empty);
        }

        private sealed class TestCommand(Func<object?, bool> canExecute) : ICommand
        {
            public event EventHandler? CanExecuteChanged;
            public int CheckCount { get; private set; }
            public int ExecuteCount { get; private set; }
            public object? LastCheckedParameter { get; private set; }
            public object? LastExecutedParameter { get; private set; }

            public bool CanExecute(object? parameter)
            {
                CheckCount++;
                LastCheckedParameter = parameter;
                return canExecute(parameter);
            }

            public void Execute(object? parameter)
            {
                ExecuteCount++;
                LastExecutedParameter = parameter;
            }

            public void NotifyCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
