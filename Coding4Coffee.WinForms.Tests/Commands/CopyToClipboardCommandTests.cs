using Coding4Coffee.WinForms.Commands;
using Coding4Coffee.WinForms.Commands.CommandEventArgs;
using Xunit;

namespace Coding4Coffee.WinForms.Tests.Commands
{
    public class CopyToClipboardCommandTests
    {
        [Fact]
        public void Constructor_NullText_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new CopyToClipboardCommand((string)null!));
        }

        [Fact]
        public void Constructor_NullTextProvider_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new CopyToClipboardCommand((Func<string?>)null!));
        }

        [Fact]
        public void Constructor_NullDataProvider_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new CopyToClipboardCommand((Func<object?>)null!));
        }

        [Fact]
        public void CanExecute_ParameterDriven_ReturnsFalseWhenNullOrEmpty()
        {
            var command = new CopyToClipboardCommand();

            Assert.False(command.CanExecute(null));
            Assert.False(command.CanExecute(string.Empty));
            Assert.True(command.CanExecute("Valid text"));
        }

        [Fact]
        public void CanExecute_FixedText_ReturnsTrueForValidText()
        {
            var command = new CopyToClipboardCommand("Hello World");

            Assert.True(command.CanExecute(null));
        }

        [Fact]
        public void CanExecute_Provider_EvaluatesDynamicValue()
        {
            string? dynamicText = null;
            var command = new CopyToClipboardCommand(() => dynamicText);

            Assert.False(command.CanExecute(null));

            dynamicText = "Some value";
            Assert.True(command.CanExecute(null));

            dynamicText = string.Empty;
            Assert.False(command.CanExecute(null));
        }

        [Fact]
        public void CanExecute_WithCustomPredicate_EvaluatesPredicate()
        {
            var command = new CopyToClipboardCommand(canExecute: param => param is string s && s.StartsWith("Allowed:"));

            Assert.False(command.CanExecute("Forbidden"));
            Assert.True(command.CanExecute("Allowed:Text"));
        }

        [Fact]
        public void Execute_CopiesData_SetsLastCopiedData_AndRaisesCopiedEvent()
        {
            object? capturedData = null;
            var command = new CopyToClipboardCommand(
                dataProvider: () => "Text from provider",
                canExecute: null,
                clipboardSetter: data => capturedData = data);

            ClipboardCopiedEventArgs? receivedArgs = null;
            command.Copied += (_, e) => receivedArgs = e;

            command.Execute(null);

            Assert.Equal("Text from provider", capturedData);
            Assert.Equal("Text from provider", command.LastCopiedData);
            Assert.NotNull(receivedArgs);
            Assert.Equal("Text from provider", receivedArgs.Text);
        }

        [Fact]
        public void Execute_ParameterOverridesProvider()
        {
            object? capturedData = null;
            var command = new CopyToClipboardCommand(
                dataProvider: () => "Provider text",
                canExecute: null,
                clipboardSetter: data => capturedData = data);

            command.Execute("Overridden text");

            Assert.Equal("Overridden text", capturedData);
            Assert.Equal("Overridden text", command.LastCopiedData);
        }

        [Fact]
        public void Execute_WhenCannotExecute_DoesNothing()
        {
            bool setterCalled = false;
            var command = new CopyToClipboardCommand(
                dataProvider: () => null,
                canExecute: null,
                clipboardSetter: _ => setterCalled = true);

            command.Execute(null);

            Assert.False(setterCalled);
            Assert.Null(command.LastCopiedData);
        }

        [Fact]
        public void RaiseCanExecuteChanged_FiresEvent()
        {
            var command = new CopyToClipboardCommand("text");
            bool eventFired = false;
            command.CanExecuteChanged += (_, _) => eventFired = true;

            command.RaiseCanExecuteChanged();

            Assert.True(eventFired);
        }
    }
}
