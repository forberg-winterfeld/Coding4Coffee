using System.Diagnostics;
using System.Windows.Input;

namespace Coding4Coffee.WinForms.Commands
{
    /// <summary>
    /// Command to reveal a file or directory in Windows Explorer.
    /// </summary>
    /// <remarks>
    /// If the target is an existing file, Windows Explorer is launched with the file selected (<c>explorer.exe /select,"path"</c>).
    /// If the target is a directory, the directory is opened in Windows Explorer.
    /// If the file does not exist but <see cref="FallbackToParentDirectory"/> is enabled, its containing directory is opened instead.
    ///
    /// Typical usage (static path):
    /// <code>
    /// var showLogCommand = new ShowInExplorerCommand(@"C:\Logs\app.log");
    /// showLogButton.BindCommand(showLogCommand);
    /// </code>
    ///
    /// Typical usage (parameter-driven):
    /// <code>
    /// var showCommand = new ShowInExplorerCommand();
    /// showCommand.Execute(selectedFilePath);
    /// </code>
    /// </remarks>
    public class ShowInExplorerCommand : ICommand
    {
        private readonly Func<string?>? _pathProvider;
        private readonly Func<object?, bool>? _canExecute;
        private readonly Action<string, bool>? _explorerLauncher;

        /// <summary>
        /// Gets a value indicating whether to open the parent directory if the specified file does not exist.
        /// </summary>
        public bool FallbackToParentDirectory { get; }

        /// <summary>
        /// Gets the most recent path opened by this command.
        /// </summary>
        public string? LastOpenedPath { get; private set; }

        /// <summary>
        /// Event that is raised when the command's ability to execute changes.
        /// </summary>
        public event EventHandler? CanExecuteChanged;

        /// <summary>
        /// Event that is raised when Windows Explorer has been launched.
        /// </summary>
        public event EventHandler<string>? Executed;

        /// <summary>
        /// Initializes a new instance of the <see cref="ShowInExplorerCommand"/> class that resolves the target path from the command parameter.
        /// </summary>
        /// <param name="fallbackToParentDirectory">Whether to fall back to the parent folder if the target file does not exist.</param>
        /// <param name="canExecute">Optional predicate to determine whether the command can execute.</param>
        public ShowInExplorerCommand(bool fallbackToParentDirectory = true, Func<object?, bool>? canExecute = null)
            : this((Func<string?>?)null, fallbackToParentDirectory, canExecute, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ShowInExplorerCommand"/> class with a fixed path.
        /// </summary>
        /// <param name="path">The file or directory path to reveal.</param>
        /// <param name="fallbackToParentDirectory">Whether to fall back to the parent folder if the target file does not exist.</param>
        /// <param name="canExecute">Optional predicate to determine whether the command can execute.</param>
        public ShowInExplorerCommand(string path, bool fallbackToParentDirectory = true, Func<object?, bool>? canExecute = null)
            : this(() => path, fallbackToParentDirectory, canExecute, null)
        {
            ArgumentNullException.ThrowIfNull(path);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ShowInExplorerCommand"/> class with a <see cref="FileSystemInfo"/>.
        /// </summary>
        /// <param name="fileSystemInfo">The file or directory info to reveal.</param>
        /// <param name="fallbackToParentDirectory">Whether to fall back to the parent folder if the target file does not exist.</param>
        /// <param name="canExecute">Optional predicate to determine whether the command can execute.</param>
        public ShowInExplorerCommand(FileSystemInfo fileSystemInfo, bool fallbackToParentDirectory = true, Func<object?, bool>? canExecute = null)
            : this(() => fileSystemInfo.FullName, fallbackToParentDirectory, canExecute, null)
        {
            ArgumentNullException.ThrowIfNull(fileSystemInfo);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ShowInExplorerCommand"/> class with a dynamic path provider.
        /// </summary>
        /// <param name="pathProvider">A function providing the path to reveal.</param>
        /// <param name="fallbackToParentDirectory">Whether to fall back to the parent folder if the target file does not exist.</param>
        /// <param name="canExecute">Optional predicate to determine whether the command can execute.</param>
        public ShowInExplorerCommand(Func<string?> pathProvider, bool fallbackToParentDirectory = true, Func<object?, bool>? canExecute = null)
            : this(pathProvider, fallbackToParentDirectory, canExecute, null)
        {
            ArgumentNullException.ThrowIfNull(pathProvider);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ShowInExplorerCommand"/> class with full configuration for testing or custom process launching.
        /// </summary>
        /// <param name="pathProvider">A function providing the path to reveal.</param>
        /// <param name="fallbackToParentDirectory">Whether to fall back to the parent folder if the target file does not exist.</param>
        /// <param name="canExecute">Optional predicate to determine whether the command can execute.</param>
        /// <param name="explorerLauncher">Custom launcher action receiving (targetPath, selectInFolder).</param>
        internal ShowInExplorerCommand(Func<string?>? pathProvider, bool fallbackToParentDirectory, Func<object?, bool>? canExecute, Action<string, bool>? explorerLauncher)
        {
            _pathProvider = pathProvider;
            FallbackToParentDirectory = fallbackToParentDirectory;
            _canExecute = canExecute;
            _explorerLauncher = explorerLauncher;
        }

        /// <summary>
        /// Determines whether the command can execute with the specified parameter.
        /// </summary>
        /// <param name="parameter">A file/directory path (<see cref="string"/>, <see cref="FileSystemInfo"/>, or <see cref="Uri"/>), or <see langword="null"/> if configured via constructor.</param>
        /// <returns><see langword="true"/> if a valid existing file or directory is found (or its parent exists when fallback is enabled) and any predicate passes; otherwise, <see langword="false"/>.</returns>
        public virtual bool CanExecute(object? parameter)
        {
            string? rawPath = ResolveRawPath(parameter);

            if (string.IsNullOrWhiteSpace(rawPath))
                return false;

            string? fullPath = TryGetFullPath(rawPath);
            if (fullPath == null)
                return false;

            if (!PathExists(fullPath, FallbackToParentDirectory))
                return false;

            return _canExecute?.Invoke(parameter) ?? true;
        }

        /// <summary>
        /// Executes the command, opening Windows Explorer to display the directory or select the specified file.
        /// </summary>
        /// <param name="parameter">A file/directory path (<see cref="string"/>, <see cref="FileSystemInfo"/>, or <see cref="Uri"/>), or <see langword="null"/> if configured via constructor.</param>
        public virtual void Execute(object? parameter)
        {
            if (!CanExecute(parameter))
                return;

            string rawPath = ResolveRawPath(parameter)!;
            string fullPath = TryGetFullPath(rawPath)!;

            (string target, bool selectInFolder) = ResolveExecutionTarget(fullPath, FallbackToParentDirectory);

            LaunchExplorer(target, selectInFolder);

            LastOpenedPath = target;
            OnExecuted(target);
        }

        /// <summary>
        /// Raises the <see cref="CanExecuteChanged"/> event.
        /// </summary>
        public void RaiseCanExecuteChanged()
            => CanExecuteChanged?.Invoke(this, EventArgs.Empty);

        /// <summary>
        /// Raises the <see cref="Executed"/> event.
        /// </summary>
        /// <param name="path">The path that was revealed or opened.</param>
        protected virtual void OnExecuted(string path)
            => Executed?.Invoke(this, path);

        private string? ResolveRawPath(object? parameter)
            => parameter switch
            {
                string path => path,
                FileSystemInfo fileSystemInfo => fileSystemInfo.FullName,
                Uri { IsFile: true } fileUri => fileUri.LocalPath,
                _ => _pathProvider?.Invoke()
            };

        private static string? TryGetFullPath(string rawPath)
        {
            try
            {
                string expandedPath = Environment.ExpandEnvironmentVariables(rawPath);
                return Path.GetFullPath(expandedPath);
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static bool PathExists(string fullPath, bool fallbackToParent)
        {
            if (File.Exists(fullPath) || Directory.Exists(fullPath))
                return true;

            if (fallbackToParent)
            {
                string? parent = Path.GetDirectoryName(fullPath);
                return !string.IsNullOrEmpty(parent) && Directory.Exists(parent);
            }

            return false;
        }

        private static (string Target, bool SelectInFolder) ResolveExecutionTarget(string fullPath, bool fallbackToParent)
        {
            if (File.Exists(fullPath))
                return (fullPath, true);

            if (Directory.Exists(fullPath))
                return (fullPath, false);

            if (fallbackToParent)
            {
                string? parent = Path.GetDirectoryName(fullPath);
                if (!string.IsNullOrEmpty(parent) && Directory.Exists(parent))
                    return (parent, false);
            }

            return (fullPath, false);
        }

        private void LaunchExplorer(string target, bool selectInFolder)
        {
            if (_explorerLauncher != null)
            {
                _explorerLauncher(target, selectInFolder);
                return;
            }

            string arguments = selectInFolder ? $"/select,\"{target}\"" : $"\"{target}\"";

            Process.Start(new ProcessStartInfo
            {
                FileName = "explorer.exe",
                Arguments = arguments,
                UseShellExecute = true
            });
        }
    }
}
