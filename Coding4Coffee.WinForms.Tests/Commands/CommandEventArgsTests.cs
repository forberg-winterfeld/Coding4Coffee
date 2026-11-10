using Coding4Coffee.WinForms.Commands.CommandEventArgs;
using Xunit;

namespace Coding4Coffee.WinForms.Tests.Commands
{
    public class CommandEventArgsTests
    {
        [Fact]
        public void MessageBoxClosedEventArgs_StoresDialogResult()
        {
            var args = new MessageBoxClosedEventArgs(DialogResult.Yes);
            Assert.Equal(DialogResult.Yes, args.DialogResult);
        }

        [Fact]
        public void TaskDialogClosedEventArgs_WithButtonTag_ReturnsTagResult()
        {
            var page = new TaskDialogPage();
            var button = new TaskDialogButton("Custom") { Tag = DialogResult.Retry };

            var args = new TaskDialogClosedEventArgs(button, page);

            Assert.Same(button, args.ClickedButton);
            Assert.Same(page, args.Page);
            Assert.Equal(DialogResult.Retry, args.DialogResult);
        }

        [Theory]
        [InlineData(nameof(TaskDialogButton.OK), DialogResult.OK)]
        [InlineData(nameof(TaskDialogButton.Cancel), DialogResult.Cancel)]
        [InlineData(nameof(TaskDialogButton.Yes), DialogResult.Yes)]
        [InlineData(nameof(TaskDialogButton.No), DialogResult.No)]
        [InlineData(nameof(TaskDialogButton.Retry), DialogResult.Retry)]
        [InlineData(nameof(TaskDialogButton.Abort), DialogResult.Abort)]
        [InlineData(nameof(TaskDialogButton.Ignore), DialogResult.Ignore)]
        public void TaskDialogClosedEventArgs_WithStandardButtons_MapsDialogResult(string buttonName, DialogResult expectedResult)
        {
            var page = new TaskDialogPage();
            TaskDialogButton standardButton = buttonName switch
            {
                nameof(TaskDialogButton.OK) => TaskDialogButton.OK,
                nameof(TaskDialogButton.Cancel) => TaskDialogButton.Cancel,
                nameof(TaskDialogButton.Yes) => TaskDialogButton.Yes,
                nameof(TaskDialogButton.No) => TaskDialogButton.No,
                nameof(TaskDialogButton.Retry) => TaskDialogButton.Retry,
                nameof(TaskDialogButton.Abort) => TaskDialogButton.Abort,
                nameof(TaskDialogButton.Ignore) => TaskDialogButton.Ignore,
                _ => throw new ArgumentException("Unknown button", nameof(buttonName))
            };

            var args = new TaskDialogClosedEventArgs(standardButton, page);

            Assert.Equal(expectedResult, args.DialogResult);
        }

        [Fact]
        public void TaskDialogClosedEventArgs_NullButton_ReturnsDialogResultNone()
        {
            var page = new TaskDialogPage();
            var args = new TaskDialogClosedEventArgs(null, page);

            Assert.Null(args.ClickedButton);
            Assert.Same(page, args.Page);
            Assert.Equal(DialogResult.None, args.DialogResult);
        }

        [Fact]
        public void TaskDialogClosedEventArgs_CustomButtonWithoutTag_ReturnsDialogResultNone()
        {
            var page = new TaskDialogPage();
            var button = new TaskDialogButton("Untagged Custom");

            var args = new TaskDialogClosedEventArgs(button, page);

            Assert.Equal(DialogResult.None, args.DialogResult);
        }

        [Fact]
        public void DialogClosedEventArgs_StoresDialogAndResult()
        {
            using var dialog = new OpenFileDialog();
            var args = new DialogClosedEventArgs<OpenFileDialog>(dialog, DialogResult.OK);

            Assert.Same(dialog, args.Dialog);
            Assert.Equal(DialogResult.OK, args.DialogResult);
        }

        [Fact]
        public void FormClosedEventArgs_StoresFormAndResult()
        {
            using var form = new Form();
            var args = new FormClosedEventArgs<Form>(form, DialogResult.Cancel);

            Assert.Same(form, args.Form);
            Assert.Equal(DialogResult.Cancel, args.DialogResult);
        }

        [Fact]
        public void ClipboardCopiedEventArgs_StoresDataAndProvidesText()
        {
            var stringArgs = new ClipboardCopiedEventArgs("sample text");
            Assert.Equal("sample text", stringArgs.Data);
            Assert.Equal("sample text", stringArgs.Text);

            var intArgs = new ClipboardCopiedEventArgs(42);
            Assert.Equal(42, intArgs.Data);
            Assert.Equal("42", intArgs.Text);

            var nullArgs = new ClipboardCopiedEventArgs(null);
            Assert.Null(nullArgs.Data);
            Assert.Null(nullArgs.Text);
        }
    }
}
