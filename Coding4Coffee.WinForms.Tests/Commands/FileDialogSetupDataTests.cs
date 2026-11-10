using Coding4Coffee.WinForms.Commands.CommandData;
using Xunit;

namespace Coding4Coffee.WinForms.Tests.Commands
{
    public class FileDialogSetupDataTests
    {
        [Fact]
        public void FolderBrowserDialogSetupData_SetupDialog_AppliesAllProperties()
        {
            var setupData = new FolderBrowserDialogSetupData
            {
                Description = "Select a folder",
                RootFolder = Environment.SpecialFolder.MyDocuments,
                SelectedPath = @"C:\Test",
                ShowNewFolderButton = true,
                AutoUpgradeEnabled = true,
                UseDescriptionForTitle = true
            };

            using var dialog = new FolderBrowserDialog();
            setupData.SetupDialog(dialog);

            Assert.Equal("Select a folder", dialog.Description);
            Assert.Equal(Environment.SpecialFolder.MyDocuments, dialog.RootFolder);
            Assert.Equal(@"C:\Test", dialog.SelectedPath);
            Assert.True(dialog.ShowNewFolderButton);
            Assert.True(dialog.AutoUpgradeEnabled);
            Assert.True(dialog.UseDescriptionForTitle);
        }

        [Fact]
        public void OpenFileDialogSetupData_SetupDialog_AppliesAllProperties()
        {
            var setupData = new OpenFileDialogSetupData
            {
                Title = "Open File Title",
                Filter = "Text files (*.txt)|*.txt",
                FileName = "test.txt",
                InitialDirectory = @"C:\",
                DefaultExt = "txt",
                Multiselect = true,
                CheckFileExists = true,
                ReadOnlyChecked = true,
                ShowReadOnly = true,
                SelectReadOnly = true,
                ShowPreview = true
            };

            using var dialog = new OpenFileDialog();
            setupData.SetupDialog(dialog);

            Assert.Equal("Open File Title", dialog.Title);
            Assert.Equal("Text files (*.txt)|*.txt", dialog.Filter);
            Assert.Equal("test.txt", dialog.FileName);
            Assert.Equal(@"C:\", dialog.InitialDirectory);
            Assert.Equal("txt", dialog.DefaultExt);
            Assert.True(dialog.Multiselect);
            Assert.True(dialog.CheckFileExists);
            Assert.True(dialog.ReadOnlyChecked);
            Assert.True(dialog.ShowReadOnly);
            Assert.True(dialog.SelectReadOnly);
            Assert.True(dialog.ShowPreview);
        }

        [Fact]
        public void SaveFileDialogSetupData_SetupDialog_AppliesAllProperties()
        {
            var setupData = new SaveFileDialogSetupData
            {
                Title = "Save File Title",
                Filter = "All files (*.*)|*.*",
                FileName = "export.csv",
                CreatePrompt = true,
                OverwritePrompt = true,
                ExpandedMode = true,
                CheckWriteAccess = true
            };

            using var dialog = new SaveFileDialog();
            setupData.SetupDialog(dialog);

            Assert.Equal("Save File Title", dialog.Title);
            Assert.Equal("All files (*.*)|*.*", dialog.Filter);
            Assert.Equal("export.csv", dialog.FileName);
            Assert.True(dialog.CreatePrompt);
            Assert.True(dialog.OverwritePrompt);
            Assert.True(dialog.ExpandedMode);
            Assert.True(dialog.CheckWriteAccess);
        }
    }
}
