using System.Diagnostics;
using System.Windows.Input;

namespace Coding4Coffee.WinForms.Commands
{
    /// <summary>
    /// Command to open a URL or URI in the default web browser or associated system application.
    /// </summary>
    /// <remarks>
    /// Resolves the target URL from a static string, a <see cref="Uri"/>, a dynamic provider, or via the command parameter.
    /// Uses <see cref="ProcessStartInfo.UseShellExecute"/> to launch the system default application.
    ///
    /// Typical usage (static URL):
    /// <code>
    /// var helpCommand = new OpenUrlCommand("https://coding4coffee.dev/docs");
    /// helpButton.BindCommand(helpCommand);
    /// </code>
    ///
    /// Typical usage (parameter-driven):
    /// <code>
    /// var openUrlCommand = new OpenUrlCommand();
    /// openUrlCommand.Execute("https://github.com");
    /// </code>
    /// </remarks>
    public class OpenUrlCommand : ICommand
    {
        private static readonly string[] DefaultSchemes = [Uri.UriSchemeHttp, Uri.UriSchemeHttps, Uri.UriSchemeMailto];

        private readonly Func<Uri?>? _uriProvider;
        private readonly Func<object?, bool>? _canExecute;
        private readonly Action<Uri>? _launcher;

        /// <summary>
        /// Gets the schemes permitted for execution by this command. Defaults to <c>http</c>, <c>https</c>, and <c>mailto</c>.
        /// </summary>
        public IReadOnlySet<string> AllowedSchemes { get; init; } = new HashSet<string>(DefaultSchemes, StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Gets the most recent <see cref="Uri"/> opened by this command.
        /// </summary>
        public Uri? LastOpenedUri { get; private set; }

        /// <summary>
        /// Event that is raised when the command's ability to execute changes.
        /// </summary>
        public event EventHandler? CanExecuteChanged;

        /// <summary>
        /// Event that is raised when a URL has been launched.
        /// </summary>
        public event EventHandler<Uri>? Executed;

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenUrlCommand"/> class that resolves the URL from the command parameter.
        /// </summary>
        /// <param name="canExecute">Optional predicate to determine whether the command can execute.</param>
        public OpenUrlCommand(Func<object?, bool>? canExecute = null)
            : this((Func<Uri?>?)null, canExecute, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenUrlCommand"/> class with a fixed URL string.
        /// </summary>
        /// <param name="url">The URL string to open.</param>
        /// <param name="canExecute">Optional predicate to determine whether the command can execute.</param>
        public OpenUrlCommand(string url, Func<object?, bool>? canExecute = null)
            : this(CreateUriProvider(url), canExecute, null)
        {
            ArgumentNullException.ThrowIfNull(url);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenUrlCommand"/> class with a fixed <see cref="Uri"/>.
        /// </summary>
        /// <param name="uri">The <see cref="Uri"/> to open.</param>
        /// <param name="canExecute">Optional predicate to determine whether the command can execute.</param>
        public OpenUrlCommand(Uri uri, Func<object?, bool>? canExecute = null)
            : this(() => uri, canExecute, null)
        {
            ArgumentNullException.ThrowIfNull(uri);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenUrlCommand"/> class with a dynamic URL string provider.
        /// </summary>
        /// <param name="urlProvider">A function providing the URL string to open.</param>
        /// <param name="canExecute">Optional predicate to determine whether the command can execute.</param>
        public OpenUrlCommand(Func<string?> urlProvider, Func<object?, bool>? canExecute = null)
            : this(CreateUriProvider(urlProvider), canExecute, null)
        {
            ArgumentNullException.ThrowIfNull(urlProvider);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenUrlCommand"/> class with a dynamic <see cref="Uri"/> provider.
        /// </summary>
        /// <param name="uriProvider">A function providing the <see cref="Uri"/> to open.</param>
        /// <param name="canExecute">Optional predicate to determine whether the command can execute.</param>
        public OpenUrlCommand(Func<Uri?> uriProvider, Func<object?, bool>? canExecute = null)
            : this(uriProvider, canExecute, null)
        {
            ArgumentNullException.ThrowIfNull(uriProvider);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenUrlCommand"/> class with full configuration for testing or custom process launching.
        /// </summary>
        /// <param name="uriProvider">A function providing the <see cref="Uri"/> to open.</param>
        /// <param name="canExecute">Optional predicate to determine whether the command can execute.</param>
        /// <param name="launcher">Custom launcher delegate for testing or custom dispatching.</param>
        internal OpenUrlCommand(Func<Uri?>? uriProvider, Func<object?, bool>? canExecute, Action<Uri>? launcher)
        {
            _uriProvider = uriProvider;
            _canExecute = canExecute;
            _launcher = launcher;
        }

        /// <summary>
        /// Determines whether the command can execute with the specified parameter.
        /// </summary>
        /// <param name="parameter">A <see cref="string"/>, <see cref="Uri"/>, or <see langword="null"/> if using a pre-configured provider.</param>
        /// <returns><see langword="true"/> if a valid URI with an allowed scheme is resolved and any configured predicate passes; otherwise, <see langword="false"/>.</returns>
        public virtual bool CanExecute(object? parameter)
        {
            Uri? uri = ResolveUri(parameter);

            if (uri == null || !uri.IsAbsoluteUri)
                return false;

            if (!AllowedSchemes.Contains(uri.Scheme))
                return false;

            return _canExecute?.Invoke(parameter) ?? true;
        }

        /// <summary>
        /// Executes the command, opening the resolved URL in the default browser.
        /// </summary>
        /// <param name="parameter">A <see cref="string"/>, <see cref="Uri"/>, or <see langword="null"/> if using a pre-configured provider.</param>
        public virtual void Execute(object? parameter)
        {
            if (!CanExecute(parameter))
                return;

            Uri uri = ResolveUri(parameter)!;

            LaunchUri(uri);

            LastOpenedUri = uri;
            OnExecuted(uri);
        }

        /// <summary>
        /// Raises the <see cref="CanExecuteChanged"/> event.
        /// </summary>
        public void RaiseCanExecuteChanged()
            => CanExecuteChanged?.Invoke(this, EventArgs.Empty);

        /// <summary>
        /// Raises the <see cref="Executed"/> event.
        /// </summary>
        /// <param name="uri">The <see cref="Uri"/> that was opened.</param>
        protected virtual void OnExecuted(Uri uri)
            => Executed?.Invoke(this, uri);

        private Uri? ResolveUri(object? parameter)
            => parameter switch
            {
                Uri uri => uri,
                string urlString => TryParseAbsoluteUri(urlString),
                _ => _uriProvider?.Invoke()
            };

        private static Func<Uri?> CreateUriProvider(string url)
            => () => TryParseAbsoluteUri(url);

        private static Func<Uri?> CreateUriProvider(Func<string?> urlProvider)
            => () =>
            {
                string? url = urlProvider();
                return string.IsNullOrWhiteSpace(url) ? null : TryParseAbsoluteUri(url);
            };

        private static Uri? TryParseAbsoluteUri(string url)
            => Uri.TryCreate(url, UriKind.Absolute, out Uri? result) ? result : null;

        private void LaunchUri(Uri uri)
        {
            if (_launcher != null)
            {
                _launcher(uri);
                return;
            }

            Process.Start(new ProcessStartInfo
            {
                FileName = uri.AbsoluteUri,
                UseShellExecute = true
            });
        }
    }
}
