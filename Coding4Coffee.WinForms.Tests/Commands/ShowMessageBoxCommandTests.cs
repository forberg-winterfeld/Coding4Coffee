using Coding4Coffee.WinForms.Commands;
using Coding4Coffee.WinForms.Commands.CommandData;
using Xunit;

namespace Coding4Coffee.WinForms.Tests.Commands
{
    public class ShowMessageBoxCommandTests
    {
        [Fact]
        public void Constructor_WithStringParameters_InitializesSetupData()
        {
            var command = new ShowMessageBoxCommand(
                text: "Hello World",
                caption: "Greeting",
                buttons: MessageBoxButtons.YesNo,
                icon: MessageBoxIcon.Information,
                defaultButton: MessageBoxDefaultButton.Button2,
                options: MessageBoxOptions.RightAlign);

            Assert.NotNull(command.SetupData);
            Assert.Equal("Hello World", command.SetupData.Text);
            Assert.Equal("Greeting", command.SetupData.Caption);
            Assert.Equal(MessageBoxButtons.YesNo, command.SetupData.Buttons);
            Assert.Equal(MessageBoxIcon.Information, command.SetupData.Icon);
            Assert.Equal(MessageBoxDefaultButton.Button2, command.SetupData.DefaultButton);
            Assert.Equal(MessageBoxOptions.RightAlign, command.SetupData.Options);
        }

        [Fact]
        public void Constructor_WithSetupData_AssignsSetupData()
        {
            var setupData = new MessageBoxSetupData
            {
                Text = "Custom Text",
                Caption = "Custom Caption",
                Buttons = MessageBoxButtons.OKCancel
            };

            var command = new ShowMessageBoxCommand(setupData);

            Assert.Same(setupData, command.SetupData);
        }

        [Fact]
        public void Constructor_NullSetupData_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new ShowMessageBoxCommand((MessageBoxSetupData)null!));
        }

        [Fact]
        public void CanExecute_DefaultPredicate_ReturnsTrue()
        {
            var command = new ShowMessageBoxCommand("Test");

            Assert.True(command.CanExecute(null));
            Assert.True(command.CanExecute("any"));
        }

        [Fact]
        public void CanExecute_CustomPredicate_EvaluatesCorrectly()
        {
            var command = new ShowMessageBoxCommand(
                new MessageBoxSetupData { Text = "Test" },
                param => param is string s && s == "allowed");

            Assert.False(command.CanExecute(null));
            Assert.False(command.CanExecute("forbidden"));
            Assert.True(command.CanExecute("allowed"));
        }

        [Fact]
        public void RaiseCanExecuteChanged_FiresEvent()
        {
            var command = new ShowMessageBoxCommand("Test");
            bool eventFired = false;
            command.CanExecuteChanged += (_, _) => eventFired = true;

            command.RaiseCanExecuteChanged();

            Assert.True(eventFired);
        }

        [Fact]
        public void MessageBoxSetupData_DefaultValues_AreCorrect()
        {
            var data = new MessageBoxSetupData();

            Assert.Equal(string.Empty, data.Text);
            Assert.Equal(string.Empty, data.Caption);
            Assert.Equal(MessageBoxButtons.OK, data.Buttons);
            Assert.Equal(MessageBoxIcon.None, data.Icon);
            Assert.Equal(MessageBoxDefaultButton.Button1, data.DefaultButton);
            Assert.Equal((MessageBoxOptions)0, data.Options);
        }
    }
}
