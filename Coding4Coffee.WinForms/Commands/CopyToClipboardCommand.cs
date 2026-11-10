using Coding4Coffee.WinForms.Commands.CommandEventArgs;
using System.Windows.Input;

namespace Coding4Coffee.WinForms.Commands
{
    /// <summary>
    /// Command to copy text or data to the Windows Clipboard safely.
    /// </summary>
    /// <remarks>
    /// Supports fixed text, dynamic data/text providers, or parameter-based copying via <see cref="ICommand.Execute(object?)"/>.
    /// Handles clipboard locking gracefully by utilizing retries.
    /// Raises the <see cref="Copied"/> event upon successful copy operations.
    ///
    /// Typical usage (parameter-driven):
    /// <code>
    /// var copyCommand = new CopyToClipboardCommand();
    /// copyButton.BindCommand(copyCommand, "Text to copy");
    /// </code>
    ///
    /// Typical usage with reactive notification:
    /// <code>
    /// var copyCommand = new CopyToClipboardCommand(() => textBox.Text);
    /// copyCommand.Copied += (_, e) => ShowNotification($"Copied: {e.Text}");
    /// </code>
    /// </remarks>
    public class CopyToClipboardCommand : ICommand
    {
        private readonly Func<object?>? _dataProvider;
        private readonly Func<object?, bool>? _canExecute;
        private readonly Action<object>? _clipboardSetter;

        /// <summary>
        /// Gets the most recent data copied to the clipboard by this command.
        /// </summary>
        public object? LastCopiedData { get; private set; }

        /// <summary>
        /// Event that is raised when the command's ability to execute changes.
        /// </summary>
        public event EventHandler? CanExecuteChanged;

        /// <summary>
        /// Event that is raised after data has been copied to the clipboard.
        /// </summary>
        public event EventHandler<ClipboardCopiedEventArgs>? Copied;

        /// <summary>
        /// Initializes a new instance of the <see cref="CopyToClipboardCommand"/> class that copies the parameter passed to <see cref="Execute(object?)"/>.
        /// </summary>
        /// <param name="canExecute">Optional predicate to determine whether the command can execute.</param>
        public CopyToClipboardCommand(Func<object?, bool>? canExecute = null)
            : this((Func<object?>?)null, canExecute, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CopyToClipboardCommand"/> class with fixed text.
        /// </summary>
        /// <param name="text">The fixed text to copy to the clipboard.</param>
        /// <param name="canExecute">Optional predicate to determine whether the command can execute.</param>
        public CopyToClipboardCommand(string text, Func<object?, bool>? canExecute = null)
            : this(() => text, canExecute, null)
        {
            ArgumentNullException.ThrowIfNull(text);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CopyToClipboardCommand"/> class with a dynamic text provider.
        /// </summary>
        /// <param name="textProvider">A function providing the text to copy.</param>
        /// <param name="canExecute">Optional predicate to determine whether the command can execute.</param>
        public CopyToClipboardCommand(Func<string?> textProvider, Func<object?, bool>? canExecute = null)
            : this((Func<object?>)textProvider, canExecute, null)
        {
            ArgumentNullException.ThrowIfNull(textProvider);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CopyToClipboardCommand"/> class with a dynamic data provider.
        /// </summary>
        /// <param name="dataProvider">A function providing the data or object to copy.</param>
        /// <param name="canExecute">Optional predicate to determine whether the command can execute.</param>
        public CopyToClipboardCommand(Func<object?> dataProvider, Func<object?, bool>? canExecute = null)
            : this(dataProvider, canExecute, null)
        {
            ArgumentNullException.ThrowIfNull(dataProvider);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CopyToClipboardCommand"/> class with full configuration for testing or custom clipboard operations.
        /// </summary>
        /// <param name="dataProvider">A function providing the data to copy.</param>
        /// <param name="canExecute">Optional predicate to determine whether the command can execute.</param>
        /// <param name="clipboardSetter">Custom clipboard setter action for testing or custom dispatching.</param>
        internal CopyToClipboardCommand(Func<object?>? dataProvider, Func<object?, bool>? canExecute, Action<object>? clipboardSetter)
        {
            _dataProvider = dataProvider;
            _canExecute = canExecute;
            _clipboardSetter = clipboardSetter;
        }

        /// <summary>
        /// Determines whether the command can execute in its current state.
        /// </summary>
        /// <param name="parameter">Data to copy if no fixed data or provider is configured, or an override parameter.</param>
        /// <returns><see langword="true"/> if data is available to copy and any configured predicate passes; otherwise, <see langword="false"/>.</returns>
        public virtual bool CanExecute(object? parameter)
        {
            object? data = ResolveData(parameter);

            if (data == null)
                return false;

            if (data is string text && string.IsNullOrEmpty(text))
                return false;

            return _canExecute?.Invoke(parameter) ?? true;
        }

        /// <summary>
        /// Executes the command, copying the resolved data to the Windows Clipboard.
        /// </summary>
        /// <param name="parameter">Data to copy if no fixed data or provider is configured, or an override parameter.</param>
        public virtual void Execute(object? parameter)
        {
            if (!CanExecute(parameter))
                return;

            object data = ResolveData(parameter)!;

            SetClipboardData(data);

            LastCopiedData = data;
            OnCopied(new ClipboardCopiedEventArgs(data));
        }

        /// <summary>
        /// Raises the <see cref="CanExecuteChanged"/> event.
        /// </summary>
        public void RaiseCanExecuteChanged()
            => CanExecuteChanged?.Invoke(this, EventArgs.Empty);

        /// <summary>
        /// Raises the <see cref="Copied"/> event.
        /// </summary>
        /// <param name="e">The event data containing the copied content.</param>
        protected virtual void OnCopied(ClipboardCopiedEventArgs e)
            => Copied?.Invoke(this, e);

        private object? ResolveData(object? parameter)
            => parameter ?? _dataProvider?.Invoke();

        private void SetClipboardData(object data)
        {
            if (_clipboardSetter != null)
            {
                _clipboardSetter(data);
                return;
            }

            if (data is IDataObject dataObject)
            {
                Clipboard.SetDataObject(dataObject, copy: true, retryTimes: 3, retryDelay: 100);
            }
            else if (data is string text)
            {
                Clipboard.SetDataObject(text, copy: true, retryTimes: 3, retryDelay: 100);
            }
            else
            {
                Clipboard.SetDataObject(data.ToString() ?? string.Empty, copy: true, retryTimes: 3, retryDelay: 100);
            }
        }
    }
}
