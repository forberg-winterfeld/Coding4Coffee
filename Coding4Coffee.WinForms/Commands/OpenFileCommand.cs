using System.Diagnostics;
using System.Windows.Input;

namespace Coding4Coffee.WinForms.Commands
{
    /// <summary>
    /// Command to open a file using its associated system application.
    /// </summary>
    /// <remarks>
    /// Resolves the target file path from a static string, a <see cref="FileInfo"/>, a dynamic provider, or via the command parameter.
    /// Supports automatic expansion of Windows environment variables (e.g. <c>%APPDATA%</c>, <c>%TEMP%</c>, <c>%USERPROFILE%</c>).
    /// Uses <see cref="ProcessStartInfo.UseShellExecute"/> to launch the file with the default program registered in the operating system.
    ///
    /// Typical usage (static path):
    /// <code>
    /// var openManualCommand = new OpenFileCommand(@"C:\Docs\manual.pdf");
    /// openManualButton.BindCommand(openManualCommand);
    /// </code>
    ///
    /// Typical usage (parameter-driven):
    /// <code>
    /// var openFileCommand = new OpenFileCommand();
    /// openFileCommand.Execute(selectedFilePath);
    /// </code>
    /// </remarks>
    public class OpenFileCommand : ICommand
    {
        private readonly Func<string?>? _filePathProvider;
        private readonly Func<object?, bool>? _canExecute;
        private readonly Action<ProcessStartInfo>? _processLauncher;

        /// <summary>
        /// Gets or sets optional command-line arguments to pass to the associated application.
        /// </summary>
        public string? Arguments { get; init; }

        /// <summary>
        /// Gets or sets the working directory for the process to be started.
        /// </summary>
        public string? WorkingDirectory { get; init; }

        /// <summary>
        /// Gets or sets the verb to use when opening the file (e.g., <c>"open"</c>, <c>"print"</c>, <c>"edit"</c>).
        /// When not set, the default action configured for the file type is performed.
        /// </summary>
        public string? Verb { get; init; }

        /// <summary>
        /// Gets the most recent file path opened by this command.
        /// </summary>
        public string? LastOpenedFilePath { get; private set; }

        /// <summary>
        /// Event that is raised when the command's ability to execute changes.
        /// </summary>
        public event EventHandler? CanExecuteChanged;

        /// <summary>
        /// Event that is raised after a file has been launched.
        /// </summary>
        public event EventHandler<string>? Executed;

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenFileCommand"/> class that resolves the target file path from the command parameter.
        /// </summary>
        /// <param name="canExecute">Optional predicate to determine whether the command can execute.</param>
        public OpenFileCommand(Func<object?, bool>? canExecute = null)
            : this((Func<string?>?)null, canExecute, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenFileCommand"/> class with a fixed file path.
        /// </summary>
        /// <param name="filePath">The path of the file to open.</param>
        /// <param name="canExecute">Optional predicate to determine whether the command can execute.</param>
        public OpenFileCommand(string filePath, Func<object?, bool>? canExecute = null)
            : this(() => filePath, canExecute, null)
        {
            ArgumentNullException.ThrowIfNull(filePath);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenFileCommand"/> class with a <see cref="FileInfo"/>.
        /// </summary>
        /// <param name="fileInfo">The <see cref="FileInfo"/> representing the file to open.</param>
        /// <param name="canExecute">Optional predicate to determine whether the command can execute.</param>
        public OpenFileCommand(FileInfo fileInfo, Func<object?, bool>? canExecute = null)
            : this(() => fileInfo.FullName, canExecute, null)
        {
            ArgumentNullException.ThrowIfNull(fileInfo);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenFileCommand"/> class with a dynamic file path provider.
        /// </summary>
        /// <param name="filePathProvider">A function providing the path of the file to open.</param>
        /// <param name="canExecute">Optional predicate to determine whether the command can execute.</param>
        public OpenFileCommand(Func<string?> filePathProvider, Func<object?, bool>? canExecute = null)
            : this(filePathProvider, canExecute, null)
        {
            ArgumentNullException.ThrowIfNull(filePathProvider);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenFileCommand"/> class with full configuration for testing or custom process launching.
        /// </summary>
        /// <param name="filePathProvider">A function providing the path of the file to open.</param>
        /// <param name="canExecute">Optional predicate to determine whether the command can execute.</param>
        /// <param name="processLauncher">Custom process launching action for testing or custom dispatching.</param>
        internal OpenFileCommand(Func<string?>? filePathProvider, Func<object?, bool>? canExecute, Action<ProcessStartInfo>? processLauncher)
        {
            _filePathProvider = filePathProvider;
            _canExecute = canExecute;
            _processLauncher = processLauncher;
        }

        /// <summary>
        /// Determines whether the command can execute with the specified parameter.
        /// </summary>
        /// <param name="parameter">A file path (<see cref="string"/>, <see cref="FileInfo"/>, or file <see cref="Uri"/>), or <see langword="null"/> if configured via constructor.</param>
        /// <returns><see langword="true"/> if a valid existing file path is resolved and any configured predicate passes; otherwise, <see langword="false"/>.</returns>
        public virtual bool CanExecute(object? parameter)
        {
            string? rawPath = ResolveRawPath(parameter);

            if (string.IsNullOrWhiteSpace(rawPath))
                return false;

            string? fullPath = TryGetFullPath(rawPath);
            if (fullPath == null)
                return false;

            if (!File.Exists(fullPath))
                return false;

            return _canExecute?.Invoke(parameter) ?? true;
        }

        /// <summary>
        /// Executes the command, opening the resolved file with its associated system application.
        /// </summary>
        /// <param name="parameter">A file path (<see cref="string"/>, <see cref="FileInfo"/>, or file <see cref="Uri"/>), or <see langword="null"/> if configured via constructor.</param>
        public virtual void Execute(object? parameter)
        {
            if (!CanExecute(parameter))
                return;

            string rawPath = ResolveRawPath(parameter)!;
            string fullPath = TryGetFullPath(rawPath)!;

            LaunchFile(fullPath);

            LastOpenedFilePath = fullPath;
            OnExecuted(fullPath);
        }

        /// <summary>
        /// Raises the <see cref="CanExecuteChanged"/> event.
        /// </summary>
        public void RaiseCanExecuteChanged()
            => CanExecuteChanged?.Invoke(this, EventArgs.Empty);

        /// <summary>
        /// Raises the <see cref="Executed"/> event.
        /// </summary>
        /// <param name="filePath">The file path that was launched.</param>
        protected virtual void OnExecuted(string filePath)
            => Executed?.Invoke(this, filePath);

        private string? ResolveRawPath(object? parameter)
            => parameter switch
            {
                string path => path,
                FileInfo fileInfo => fileInfo.FullName,
                Uri { IsFile: true } fileUri => fileUri.LocalPath,
                _ => _filePathProvider?.Invoke()
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

        private void LaunchFile(string fullPath)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = fullPath,
                UseShellExecute = true
            };

            if (!string.IsNullOrWhiteSpace(Arguments))
                startInfo.Arguments = Arguments;

            if (!string.IsNullOrWhiteSpace(WorkingDirectory))
                startInfo.WorkingDirectory = WorkingDirectory;

            if (!string.IsNullOrWhiteSpace(Verb))
                startInfo.Verb = Verb;

            if (_processLauncher != null)
            {
                _processLauncher(startInfo);
                return;
            }

            Process.Start(startInfo);
        }
    }
}
