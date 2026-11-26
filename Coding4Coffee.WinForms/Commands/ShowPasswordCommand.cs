using Coding4Coffee.Basics.Commands;

namespace Coding4Coffee.WinForms.Commands
{
    /// <summary>
    /// Temporarily reveals the text of a password <see cref="TextBox"/>.
    /// </summary>
    /// <remarks>
    /// Execute this command on the text box's owning UI thread, with its handle created and a running WinForms
    /// message loop. While executing, subsequent invocations are ignored. The text is masked with the configured
    /// character when the delay ends or cancellation is requested. The command disables
    /// <see cref="TextBox.UseSystemPasswordChar"/>; it does not restore the previous masking settings.
    /// Mask the text box before the command is first executed. Cancellation and execution errors, including
    /// invalid execution context or a disposed text box, are reported through <see cref="AsyncCommandBase.ExecutionFailed"/>.
    /// Use a single command for each text box and do not change its masking settings while it is executing.
    /// </remarks>
    public sealed class ShowPasswordCommand : AsyncCommandBase
    {
        private readonly TextBox _textBox;
        private readonly int _delayMilliseconds;
        private readonly char _passwordChar;

        /// <summary>
        /// Initializes a command that temporarily reveals the specified text box's contents.
        /// </summary>
        /// <param name="textBox">The text box whose contents will be revealed.</param>
        /// <param name="delayMilliseconds">The time in milliseconds to reveal the text. Defaults to 5000 and must be positive.</param>
        /// <param name="passwordChar">The non-null character used to mask the text afterwards. Defaults to '*'.</param>
        /// <exception cref="ArgumentNullException"><paramref name="textBox"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="delayMilliseconds"/> is not positive, or <paramref name="passwordChar"/> is the null character.</exception>
        public ShowPasswordCommand(TextBox textBox, int delayMilliseconds = 5000, char passwordChar = '*')
        {
            ArgumentNullException.ThrowIfNull(textBox);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(delayMilliseconds);
            if (passwordChar == '\0')
                throw new ArgumentOutOfRangeException(nameof(passwordChar), "The password character must not be the null character.");

            _textBox = textBox;
            _delayMilliseconds = delayMilliseconds;
            _passwordChar = passwordChar;
        }

        /// <inheritdoc />
        protected override async Task ExecuteAsync(object? _, CancellationToken cancellationToken)
        {
            ObjectDisposedException.ThrowIf(_textBox.IsDisposed || _textBox.Disposing, _textBox);
            if (!_textBox.IsHandleCreated || _textBox.InvokeRequired || !Application.MessageLoop || SynchronizationContext.Current is not WindowsFormsSynchronizationContext)
                throw new InvalidOperationException("The command must execute on the text box's UI thread with a created handle and a running WinForms message loop.");

            try
            {
                _textBox.PasswordChar = '\0';
                _textBox.UseSystemPasswordChar = false;
                await Task.Delay(_delayMilliseconds, cancellationToken);
            }
            finally
            {
                if (!_textBox.IsDisposed && !_textBox.Disposing)
                {
                    _textBox.UseSystemPasswordChar = false;
                    _textBox.PasswordChar = _passwordChar;
                }
            }
        }
    }
}
