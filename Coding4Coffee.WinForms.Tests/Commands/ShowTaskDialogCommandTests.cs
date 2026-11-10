using Coding4Coffee.Basics.Localization;
using Coding4Coffee.WinForms.Commands;
using Coding4Coffee.WinForms.Commands.CommandData;
using Xunit;

namespace Coding4Coffee.WinForms.Tests.Commands
{
    public class ShowTaskDialogCommandTests
    {
        [Fact]
        public void Constructor_WithStringParameters_InitializesSetupData()
        {
            var command = new ShowTaskDialogCommand(
                heading: "Heading Text",
                text: "Body Text",
                caption: "Window Caption",
                icon: TaskDialogIcon.Warning,
                buttons: MessageBoxButtons.YesNo);

            Assert.NotNull(command.SetupData);
            Assert.Equal("Heading Text", command.SetupData.Heading?.ToString());
            Assert.Equal("Body Text", command.SetupData.Text?.ToString());
            Assert.Equal("Window Caption", command.SetupData.Caption?.ToString());
            Assert.Equal(TaskDialogIcon.Warning, command.SetupData.Icon);
            Assert.Equal(MessageBoxButtons.YesNo, command.SetupData.Buttons);
        }

        [Fact]
        public void Constructor_WithLocalizableStrings_InitializesSetupData()
        {
            var heading = new LocalizableString("Key_Heading", "Default Heading");
            var text = new LocalizableString("Key_Text", "Default Text");
            var caption = new LocalizableString("Key_Caption", "Default Caption");

            var command = new ShowTaskDialogCommand(
                heading: heading,
                text: text,
                caption: caption,
                icon: TaskDialogIcon.Information,
                buttons: MessageBoxButtons.OKCancel);

            Assert.NotNull(command.SetupData);
            Assert.Same(heading, command.SetupData.Heading);
            Assert.Same(text, command.SetupData.Text);
            Assert.Same(caption, command.SetupData.Caption);
            Assert.Equal(TaskDialogIcon.Information, command.SetupData.Icon);
            Assert.Equal(MessageBoxButtons.OKCancel, command.SetupData.Buttons);
        }

        [Fact]
        public void Constructor_WithSetupData_AssignsSetupData()
        {
            var setupData = new TaskDialogSetupData
            {
                Heading = new LocalizableString("Key", "Fallback"),
                Buttons = MessageBoxButtons.RetryCancel
            };

            var command = new ShowTaskDialogCommand(setupData);

            Assert.Same(setupData, command.SetupData);
        }

        [Fact]
        public void Constructor_NullSetupData_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new ShowTaskDialogCommand((TaskDialogSetupData)null!));
        }

        [Fact]
        public void CanExecute_DefaultPredicate_ReturnsTrue()
        {
            var command = new ShowTaskDialogCommand("Test");

            Assert.True(command.CanExecute(null));
            Assert.True(command.CanExecute(new object()));
        }

        [Fact]
        public void CanExecute_CustomPredicate_EvaluatesCorrectly()
        {
            var command = new ShowTaskDialogCommand(
                new TaskDialogSetupData { Heading = new LocalizableString("H", "H") },
                param => param is int n && n > 0);

            Assert.False(command.CanExecute(null));
            Assert.False(command.CanExecute(-1));
            Assert.True(command.CanExecute(42));
        }

        [Fact]
        public void RaiseCanExecuteChanged_FiresEvent()
        {
            var command = new ShowTaskDialogCommand("Test");
            bool fired = false;
            command.CanExecuteChanged += (_, _) => fired = true;

            command.RaiseCanExecuteChanged();

            Assert.True(fired);
        }
    }
}
