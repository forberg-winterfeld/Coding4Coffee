using Coding4Coffee.WinForms.Commands.CommandData;
using Coding4Coffee.WinForms.Commands.CommandEventArgs;
using System.Windows.Input;

namespace Coding4Coffee.WinForms.Commands
{
    /// <summary>
    /// Command to display a <see cref="MessageBox"/>.
    /// Manages the configuration, execution, result capture, and event notification for message dialogs.
    /// </summary>
    /// <remarks>
    /// Opens a modal message box using <see cref="MessageBox.Show(IWin32Window, string, string, MessageBoxButtons, MessageBoxIcon, MessageBoxDefaultButton, MessageBoxOptions)"/>
    /// or its overload without an owner. The result is available via <see cref="LastDialogResult"/>.
    /// Optionally, subscribers can listen to the <see cref="MessageBoxClosed"/> event.
    ///
    /// Typical usage (simple alert):
    /// <code>
    /// var alertCommand = new ShowMessageBoxCommand("Data saved successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
    /// alertCommand.Execute(ownerForm);
    /// </code>
    ///
    /// Typical usage (confirmation with reactive event):
    /// <code>
    /// var confirmCommand = new ShowMessageBoxCommand("Are you sure you want to delete this item?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
    /// confirmCommand.MessageBoxClosed += (_, e) =>
    /// {
    ///     if (e.DialogResult == DialogResult.Yes)
    ///         DeleteItem();
    /// };
    /// confirmCommand.Execute(ownerForm);
    /// </code>
    /// </remarks>
    public class ShowMessageBoxCommand : ICommand
    {
        private readonly MessageBoxSetupData _setupData;
        private readonly Func<object?, bool>? _canExecute;

        /// <summary>
        /// Gets the setup data configuring this message box.
        /// </summary>
        public MessageBoxSetupData SetupData => _setupData;

        /// <summary>
        /// Gets the most recent dialog result from the last execution.
        /// </summary>
        public DialogResult LastDialogResult { get; private set; }

        /// <summary>
        /// Event that is raised when the command's ability to execute changes.
        /// </summary>
        public event EventHandler? CanExecuteChanged;

        /// <summary>
        /// Event that is raised when a message box closes after execution.
        /// </summary>
        public event EventHandler<MessageBoxClosedEventArgs>? MessageBoxClosed;

        /// <summary>
        /// Initializes a new instance of the <see cref="ShowMessageBoxCommand"/> class with a fixed message and optional title, buttons, and icon.
        /// </summary>
        /// <param name="text">The message text to display.</param>
        /// <param name="caption">The title bar text to display.</param>
        /// <param name="buttons">The buttons to display in the message box.</param>
        /// <param name="icon">The icon to display in the message box.</param>
        /// <param name="defaultButton">The default button for the message box.</param>
        /// <param name="options">Options for the message box.</param>
        public ShowMessageBoxCommand(
            string text,
            string caption = "",
            MessageBoxButtons buttons = MessageBoxButtons.OK,
            MessageBoxIcon icon = MessageBoxIcon.None,
            MessageBoxDefaultButton defaultButton = MessageBoxDefaultButton.Button1,
            MessageBoxOptions options = 0)
            : this(new MessageBoxSetupData
            {
                Text = text,
                Caption = caption,
                Buttons = buttons,
                Icon = icon,
                DefaultButton = defaultButton,
                Options = options
            })
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ShowMessageBoxCommand"/> class with a <see cref="MessageBoxSetupData"/> instance.
        /// </summary>
        /// <param name="setupData">The setup data specifying the message box appearance and options.</param>
        /// <param name="canExecute">Optional function that evaluates the parameter to determine whether the command can execute.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="setupData"/> is <see langword="null"/>.</exception>
        public ShowMessageBoxCommand(MessageBoxSetupData setupData, Func<object?, bool>? canExecute = null)
        {
            ArgumentNullException.ThrowIfNull(setupData);
            _setupData = setupData;
            _canExecute = canExecute;
        }

        /// <summary>
        /// Returns whether the command can execute in its current state.
        /// </summary>
        /// <param name="parameter">An optional parameter that can influence execution eligibility.</param>
        /// <returns><see langword="true"/> if the command can execute; otherwise, <see langword="false"/>.</returns>
        public virtual bool CanExecute(object? parameter)
            => _canExecute?.Invoke(parameter) ?? true;

        /// <summary>
        /// Executes the command, displaying the message box and capturing the result.
        /// </summary>
        /// <remarks>
        /// If <paramref name="parameter"/> is an <see cref="IWin32Window"/>, it is used as the modal owner of the message box.
        /// After the dialog closes, <see cref="LastDialogResult"/> is updated and <see cref="MessageBoxClosed"/> is raised.
        /// </remarks>
        /// <param name="parameter">An optional owner window or custom execution parameter.</param>
        public virtual void Execute(object? parameter)
        {
            if (!CanExecute(parameter))
                return;

            IWin32Window? owner = parameter as IWin32Window;

            LastDialogResult = owner is not null
                ? MessageBox.Show(owner, _setupData.Text, _setupData.Caption, _setupData.Buttons, _setupData.Icon, _setupData.DefaultButton, _setupData.Options)
                : MessageBox.Show(_setupData.Text, _setupData.Caption, _setupData.Buttons, _setupData.Icon, _setupData.DefaultButton, _setupData.Options);

            OnMessageBoxClosed(new MessageBoxClosedEventArgs(LastDialogResult));
        }

        /// <summary>
        /// Raises the <see cref="MessageBoxClosed"/> event.
        /// </summary>
        /// <param name="e">Event arguments containing the dialog result.</param>
        protected virtual void OnMessageBoxClosed(MessageBoxClosedEventArgs e)
            => MessageBoxClosed?.Invoke(this, e);

        /// <summary>
        /// Raises the <see cref="CanExecuteChanged"/> event.
        /// </summary>
        public virtual void RaiseCanExecuteChanged()
            => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
