using Coding4Coffee.WinForms.Commands;
using Xunit;

namespace Coding4Coffee.WinForms.Tests.Commands
{
    public class ShowInExplorerCommandTests
    {
        [Fact]
        public void Constructor_NullPath_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new ShowInExplorerCommand((string)null!));
        }

        [Fact]
        public void Constructor_NullFileSystemInfo_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new ShowInExplorerCommand((FileSystemInfo)null!));
        }

        [Fact]
        public void Constructor_NullPathProvider_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new ShowInExplorerCommand((Func<string?>)null!));
        }

        [Fact]
        public void CanExecute_NullOrWhitespace_ReturnsFalse()
        {
            var command = new ShowInExplorerCommand();

            Assert.False(command.CanExecute(null));
            Assert.False(command.CanExecute(string.Empty));
            Assert.False(command.CanExecute("   "));
        }

        [Fact]
        public void CanExecute_ExistingDirectory_ReturnsTrue()
        {
            var command = new ShowInExplorerCommand();
            string tempDir = Path.GetTempPath();

            Assert.True(command.CanExecute(tempDir));
            Assert.True(command.CanExecute(new DirectoryInfo(tempDir)));
        }

        [Fact]
        public void CanExecute_ExistingFile_ReturnsTrue()
        {
            string tempFile = Path.GetTempFileName();
            try
            {
                var command = new ShowInExplorerCommand(tempFile);

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
            var command = new ShowInExplorerCommand(@"%TEMP%");

            Assert.True(command.CanExecute(null));
            Assert.True(command.CanExecute(@"%USERPROFILE%"));
            Assert.True(command.CanExecute(@"%APPDATA%"));
        }

        [Fact]
        public void Execute_WithEnvironmentVariables_ExpandsPathForExplorer()
        {
            string? launchedTarget = null;
            var command = new ShowInExplorerCommand(
                pathProvider: () => @"%TEMP%",
                fallbackToParentDirectory: true,
                canExecute: null,
                explorerLauncher: (target, _) => launchedTarget = target);

            command.Execute(null);

            string expectedTemp = Path.GetFullPath(Environment.GetEnvironmentVariable("TEMP")!);
            Assert.NotNull(launchedTarget);
            Assert.Equal(expectedTemp, launchedTarget);
        }

        [Fact]
        public void CanExecute_NonExistingFile_WithFallbackEnabled_ReturnsTrueWhenParentExists()
        {
            string nonExistentFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "test.txt");
            string parentDir = Path.GetDirectoryName(nonExistentFile)!;
            Directory.CreateDirectory(parentDir);

            try
            {
                var commandWithFallback = new ShowInExplorerCommand(fallbackToParentDirectory: true);
                var commandWithoutFallback = new ShowInExplorerCommand(fallbackToParentDirectory: false);

                Assert.True(commandWithFallback.CanExecute(nonExistentFile));
                Assert.False(commandWithoutFallback.CanExecute(nonExistentFile));
            }
            finally
            {
                if (Directory.Exists(parentDir))
                    Directory.Delete(parentDir, true);
            }
        }

        [Fact]
        public void CanExecute_CustomPredicate_EvaluatesPredicate()
        {
            string tempDir = Path.GetTempPath();
            var command = new ShowInExplorerCommand(tempDir, canExecute: _ => false);

            Assert.False(command.CanExecute(null));
        }

        [Fact]
        public void Execute_ExistingFile_SelectsFileInExplorer()
        {
            string tempFile = Path.GetTempFileName();
            string? launchedTarget = null;
            bool? wasSelected = null;

            try
            {
                var command = new ShowInExplorerCommand(
                    pathProvider: () => tempFile,
                    fallbackToParentDirectory: true,
                    canExecute: null,
                    explorerLauncher: (target, select) =>
                    {
                        launchedTarget = target;
                        wasSelected = select;
                    });

                string? eventTarget = null;
                command.Executed += (_, target) => eventTarget = target;

                command.Execute(null);

                Assert.Equal(tempFile, launchedTarget);
                Assert.True(wasSelected);
                Assert.Equal(tempFile, command.LastOpenedPath);
                Assert.Equal(tempFile, eventTarget);
            }
            finally
            {
                if (File.Exists(tempFile))
                    File.Delete(tempFile);
            }
        }

        [Fact]
        public void Execute_ExistingDirectory_OpensFolderWithoutSelection()
        {
            string tempDir = Path.GetTempPath();
            string? launchedTarget = null;
            bool? wasSelected = null;

            var command = new ShowInExplorerCommand(
                pathProvider: () => tempDir,
                fallbackToParentDirectory: true,
                canExecute: null,
                explorerLauncher: (target, select) =>
                {
                    launchedTarget = target;
                    wasSelected = select;
                });

            command.Execute(null);

            Assert.Equal(tempDir, launchedTarget);
            Assert.False(wasSelected);
            Assert.Equal(tempDir, command.LastOpenedPath);
        }

        [Fact]
        public void Execute_NonExistingFile_WithFallback_OpensParentFolder()
        {
            string nonExistentFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "file.txt");
            string parentDir = Path.GetDirectoryName(nonExistentFile)!;
            Directory.CreateDirectory(parentDir);

            try
            {
                string? launchedTarget = null;
                bool? wasSelected = null;

                var command = new ShowInExplorerCommand(
                    pathProvider: () => nonExistentFile,
                    fallbackToParentDirectory: true,
                    canExecute: null,
                    explorerLauncher: (target, select) =>
                    {
                        launchedTarget = target;
                        wasSelected = select;
                    });

                command.Execute(null);

                Assert.Equal(parentDir, launchedTarget);
                Assert.False(wasSelected);
            }
            finally
            {
                if (Directory.Exists(parentDir))
                    Directory.Delete(parentDir, true);
            }
        }

        [Fact]
        public void Execute_WhenCannotExecute_DoesNothing()
        {
            bool launcherCalled = false;
            var command = new ShowInExplorerCommand(
                pathProvider: () => null,
                fallbackToParentDirectory: true,
                canExecute: null,
                explorerLauncher: (_, _) => launcherCalled = true);

            command.Execute(null);

            Assert.False(launcherCalled);
            Assert.Null(command.LastOpenedPath);
        }

        [Fact]
        public void RaiseCanExecuteChanged_FiresEvent()
        {
            var command = new ShowInExplorerCommand(Path.GetTempPath());
            bool eventFired = false;
            command.CanExecuteChanged += (_, _) => eventFired = true;

            command.RaiseCanExecuteChanged();

            Assert.True(eventFired);
        }
    }
}
