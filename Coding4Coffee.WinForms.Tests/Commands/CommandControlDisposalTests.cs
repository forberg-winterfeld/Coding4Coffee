using System.ComponentModel;
using System.Windows.Input;
using Coding4Coffee.WinForms.Commands.Controls;

namespace Coding4Coffee.WinForms.Tests.Commands
{
    public class CommandControlDisposalTests
    {
        public static TheoryData<Type, string> CommandControls => new()
        {
            { typeof(DateTimePickerWithCommandSupport), "Command" },
            { typeof(NumericUpDownWithCommandSupport), "Command" },
            { typeof(TrackBarWithCommandSupport), "Command" },
            { typeof(LocalizableCheckBoxWithCommandSupport), "Command" },
            { typeof(LocalizableComboBoxWithCommandSupport), "Command" },
            { typeof(LocalizableDomainUpDownWithCommandSupport), "Command" },
            { typeof(LocalizableLinkLabelWithCommandSupport), "Command" },
            { typeof(LocalizableListBoxWithCommandSupport), "Command" },
            { typeof(LocalizableMaskedTextBoxWithCommandSupport), "TextCommand" },
            { typeof(LocalizableRadioButtonWithCommandSupport), "Command" },
            { typeof(LocalizableRichTextBoxWithCommandSupport), "Command" },
            { typeof(LocalizableTabControlWithCommandSupport), "Command" },
            { typeof(LocalizableTextBoxWithCommandSupport), "Command" },
            { typeof(LocalizableToolStripComboBoxWithCommandSupport), "Command" },
            { typeof(LocalizableToolStripSplitButtonWithCommandSupport), "Command" },
            { typeof(LocalizableToolStripTextBoxWithCommandSupport), "Command" }
        };

        [Theory]
        [MemberData(nameof(CommandControls))]
        public void Dispose_UnsubscribesCurrentCommandAndAllowsRepeatedDisposal(Type controlType, string propertyName)
        {
            using var control = Assert.IsAssignableFrom<Component>(Activator.CreateInstance(controlType));
            var property = controlType.GetProperty(propertyName)!;
            var first = new TestCommand();
            var current = new TestCommand();

            property.SetValue(control, first);
            Assert.Equal(1, first.SubscriberCount);
            property.SetValue(control, current);
            Assert.Equal(0, first.SubscriberCount);
            Assert.Equal(1, current.SubscriberCount);

            control.Dispose();
            control.Dispose();

            Assert.Equal(0, current.SubscriberCount);
            Assert.Null(property.GetValue(control));
            int checks = current.CanExecuteCount;
            current.Notify();
            Assert.Equal(checks, current.CanExecuteCount);
        }

        private sealed class TestCommand : ICommand
        {
            private EventHandler? _canExecuteChanged;

            public event EventHandler? CanExecuteChanged
            {
                add => _canExecuteChanged += value;
                remove => _canExecuteChanged -= value;
            }

            public int SubscriberCount => _canExecuteChanged?.GetInvocationList().Length ?? 0;
            public int CanExecuteCount { get; private set; }

            public bool CanExecute(object? parameter)
            {
                CanExecuteCount++;
                return true;
            }

            public void Execute(object? parameter) { }

            public void Notify() => _canExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
