using Coding4Coffee.Basics.Localization;
using Coding4Coffee.WinForms.Commands.CommandData;
using Xunit;

namespace Coding4Coffee.WinForms.Tests.Commands
{
    public class TaskDialogSetupDataTests
    {
        [Fact]
        public void SetupPage_NullPage_ThrowsArgumentNullException()
        {
            var setupData = new TaskDialogSetupData();
            Assert.Throws<ArgumentNullException>(() => setupData.SetupPage(null!));
        }

        [Fact]
        public void SetupPage_AppliesPropertiesCorrectly()
        {
            var setupData = new TaskDialogSetupData
            {
                Caption = new LocalizableString("Key_Cap", "Caption text"),
                Heading = new LocalizableString("Key_Head", "Heading text"),
                Text = new LocalizableString("Key_Text", "Body text"),
                Icon = TaskDialogIcon.Information,
                AllowCancel = true,
                AllowMinimize = true,
                EnableLinks = true,
                SizeToContent = true,
                ExpanderText = new LocalizableString("Key_Exp", "More info"),
                VerificationText = new LocalizableString("Key_Ver", "Do not show again"),
                VerificationChecked = true
            };

            var page = new TaskDialogPage();
            setupData.SetupPage(page);

            Assert.Equal("Caption text", page.Caption);
            Assert.Equal("Heading text", page.Heading);
            Assert.Equal("Body text", page.Text);
            Assert.Equal(TaskDialogIcon.Information, page.Icon);
            Assert.True(page.AllowCancel);
            Assert.True(page.AllowMinimize);
            Assert.True(page.EnableLinks);
            Assert.True(page.SizeToContent);
            Assert.NotNull(page.Expander);
            Assert.Equal("More info", page.Expander.Text);
            Assert.NotNull(page.Verification);
            Assert.Equal("Do not show again", page.Verification.Text);
            Assert.True(page.Verification.Checked);
        }

        [Fact]
        public void SetupPage_AppliesCustomButtons_WhenSpecified()
        {
            var customButton1 = new TaskDialogButton("Button 1");
            var customButton2 = new TaskDialogButton("Button 2");

            var setupData = new TaskDialogSetupData
            {
                CustomButtons = new List<TaskDialogButton> { customButton1, customButton2 }
            };

            var page = new TaskDialogPage();
            setupData.SetupPage(page);

            Assert.Equal(2, page.Buttons.Count);
            Assert.Contains(customButton1, page.Buttons);
            Assert.Contains(customButton2, page.Buttons);
        }

        [Fact]
        public void SetupPage_AppliesStandardButtons_WhenNoCustomButtons()
        {
            var setupData = new TaskDialogSetupData
            {
                Buttons = MessageBoxButtons.YesNo
            };

            var page = new TaskDialogPage();
            setupData.SetupPage(page);

            Assert.Equal(2, page.Buttons.Count);
        }

        [Theory]
        [InlineData(MessageBoxButtons.OK, 1, new[] { DialogResult.OK })]
        [InlineData(MessageBoxButtons.OKCancel, 2, new[] { DialogResult.OK, DialogResult.Cancel })]
        [InlineData(MessageBoxButtons.YesNo, 2, new[] { DialogResult.Yes, DialogResult.No })]
        [InlineData(MessageBoxButtons.YesNoCancel, 3, new[] { DialogResult.Yes, DialogResult.No, DialogResult.Cancel })]
        [InlineData(MessageBoxButtons.RetryCancel, 2, new[] { DialogResult.Retry, DialogResult.Cancel })]
        [InlineData(MessageBoxButtons.AbortRetryIgnore, 3, new[] { DialogResult.Abort, DialogResult.Retry, DialogResult.Ignore })]
        [InlineData(MessageBoxButtons.CancelTryContinue, 3, new[] { DialogResult.Cancel, DialogResult.Retry, DialogResult.Ignore })]
        public void CreateLocalizedButtons_CreatesExpectedButtonsAndTags(MessageBoxButtons buttons, int expectedCount, DialogResult[] expectedResults)
        {
            var result = TaskDialogSetupData.CreateLocalizedButtons(buttons);

            Assert.Equal(expectedCount, result.Count);
            for (int i = 0; i < expectedCount; i++)
            {
                Assert.Equal(expectedResults[i], result[i].Tag);
            }
        }

        [Fact]
        public void CreateLocalizedButtons_UsesCustomKeys_WhenProvided()
        {
            var customKeys = new DialogButtonKeys
            {
                Yes = "Custom_Yes_Key",
                No = "Custom_No_Key"
            };

            var buttons = TaskDialogSetupData.CreateLocalizedButtons(MessageBoxButtons.YesNo, customKeys);

            Assert.Equal(2, buttons.Count);
            Assert.Equal(DialogResult.Yes, buttons[0].Tag);
            Assert.Equal(DialogResult.No, buttons[1].Tag);
        }

        [Fact]
        public void DialogButtonKeys_Default_HasStandardValues()
        {
            var keys = DialogButtonKeys.Default;

            Assert.NotNull(keys);
            Assert.Equal("Dialog_OK", keys.OK);
            Assert.Equal("Dialog_Cancel", keys.Cancel);
            Assert.Equal("Dialog_Yes", keys.Yes);
            Assert.Equal("Dialog_No", keys.No);
            Assert.Equal("Dialog_Retry", keys.Retry);
            Assert.Equal("Dialog_Abort", keys.Abort);
            Assert.Equal("Dialog_Ignore", keys.Ignore);
            Assert.Equal("Dialog_TryAgain", keys.TryAgain);
            Assert.Equal("Dialog_Continue", keys.Continue);
        }
    }
}
