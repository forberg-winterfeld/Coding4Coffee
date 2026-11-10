using Coding4Coffee.WinForms.Commands;
using System.Diagnostics;
using Xunit;

namespace Coding4Coffee.WinForms.Tests.Commands
{
    public class OpenFileCommandTests
    {
        [Fact]
        public void Constructor_NullPath_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new OpenFileCommand((string)null!));
        }

        [Fact]
        public void Constructor_NullFileInfo_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new OpenFileCommand((FileInfo)null!));
        }

        [Fact]
        public void Constructor_NullPathProvider_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new OpenFileCommand((Func<string?>)null!));
        }

        [Fact]
        public void CanExecute_NullOrWhitespace_ReturnsFalse()
        {
            var command = new OpenFileCommand();

            Assert.False(command.CanExecute(null));
            Assert.False(command.CanExecute(string.Empty));
            Assert.False(command.CanExecute("   "));
        }

        [Fact]
        public void CanExecute_NonExistentFile_ReturnsFalse()
        {
            var command = new OpenFileCommand(@"C:\non_existent_folder_xyz\test.txt");

            Assert.False(command.CanExecute(null));
        }

        [Fact]
        public void CanExecute_DirectoryPath_ReturnsFalse()
        {
            string tempDir = Path.GetTempPath();
            var command = new OpenFileCommand(tempDir);

            Assert.False(command.CanExecute(null));
        }

        [Fact]
        public void CanExecute_ExistingFile_ReturnsTrue()
        {
            string tempFile = Path.GetTempFileName();
            try
            {
                var command = new OpenFileCommand(tempFile);

                Assert.True(command.CanExecute(null));
                Assert.True(command.CanExecute(new FileInfo(tempFile)));
                Assert.True(command.CanExecute(new Uri(tempFile)));
            }
            finally
            {
                if (File.Exists(tempFile))
                    File.Delete(tempFile);
            }
        }

        [Fact]
        public void CanExecute_WithEnvironmentVariables_ExpandsAndValidatesPath()
        {
            string tempFile = Path.GetTempFileName();
            try
            {
                string fileName = Path.GetFileName(tempFile);
                string envPath = Path.Combine("%TEMP%", fileName);

                var command = new OpenFileCommand(envPath);

                Assert.True(command.CanExecute(null));
            }
            finally
            {
                if (File.Exists(tempFile))
                    File.Delete(tempFile);
            }
        }

        [Fact]
        public void CanExecute_DynamicProvider_EvaluatesProvider()
        {
            string? currentPath = null;
            var command = new OpenFileCommand(() => currentPath);

            Assert.False(command.CanExecute(null));

            string tempFile = Path.GetTempFileName();
            try
            {
                currentPath = tempFile;
                Assert.True(command.CanExecute(null));
            }
            finally
            {
                if (File.Exists(tempFile))
                    File.Delete(tempFile);
            }
        }

        [Fact]
        public void CanExecute_CustomPredicate_EvaluatesPredicate()
        {
            string tempFile = Path.GetTempFileName();
            try
            {
                var command = new OpenFileCommand(tempFile, canExecute: _ => false);

                Assert.False(command.CanExecute(null));
            }
            finally
            {
                if (File.Exists(tempFile))
                    File.Delete(tempFile);
            }
        }

        [Fact]
        public void Execute_LaunchesProcess_SetsLastOpenedFilePath_AndRaisesExecutedEvent()
        {
            string tempFile = Path.GetTempFileName();
            try
            {
                ProcessStartInfo? capturedStartInfo = null;
                var command = new OpenFileCommand(
                    filePathProvider: () => tempFile,
                    canExecute: null,
                    processLauncher: psi => capturedStartInfo = psi);

                string? eventFilePath = null;
                command.Executed += (_, path) => eventFilePath = path;

                command.Execute(null);

                Assert.NotNull(capturedStartInfo);
                Assert.Equal(tempFile, capturedStartInfo.FileName);
                Assert.True(capturedStartInfo.UseShellExecute);
                Assert.Equal(tempFile, command.LastOpenedFilePath);
                Assert.Equal(tempFile, eventFilePath);
            }
            finally
            {
                if (File.Exists(tempFile))
                    File.Delete(tempFile);
            }
        }

        [Fact]
        public void Execute_WithOptions_PopulatesProcessStartInfo()
        {
            string tempFile = Path.GetTempFileName();
            try
            {
                ProcessStartInfo? capturedStartInfo = null;
                var command = new OpenFileCommand(
                    filePathProvider: () => tempFile,
                    canExecute: null,
                    processLauncher: psi => capturedStartInfo = psi)
                {
                    Arguments = "--readonly --page 1",
                    WorkingDirectory = Path.GetTempPath(),
                    Verb = "open"
                };

                command.Execute(null);

                Assert.NotNull(capturedStartInfo);
                Assert.Equal("--readonly --page 1", capturedStartInfo.Arguments);
                Assert.Equal(Path.GetTempPath(), capturedStartInfo.WorkingDirectory);
                Assert.Equal("open", capturedStartInfo.Verb);
            }
            finally
            {
                if (File.Exists(tempFile))
                    File.Delete(tempFile);
            }
        }

        [Fact]
        public void Execute_WhenCannotExecute_DoesNothing()
        {
            bool launcherCalled = false;
            var command = new OpenFileCommand(
                filePathProvider: () => null,
                canExecute: null,
                processLauncher: _ => launcherCalled = true);

            command.Execute(null);

            Assert.False(launcherCalled);
            Assert.Null(command.LastOpenedFilePath);
        }

        [Fact]
        public void RaiseCanExecuteChanged_FiresEvent()
        {
            var command = new OpenFileCommand("dummy.txt");
            bool eventFired = false;
            command.CanExecuteChanged += (_, _) => eventFired = true;

            command.RaiseCanExecuteChanged();

            Assert.True(eventFired);
        }
    }
}
