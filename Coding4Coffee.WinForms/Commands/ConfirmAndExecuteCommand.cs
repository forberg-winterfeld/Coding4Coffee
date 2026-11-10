using Coding4Coffee.Basics.Localization;
using Coding4Coffee.WinForms.Commands.CommandData;
using System.Windows.Input;

namespace Coding4Coffee.WinForms.Commands
{
    /// <summary>
    /// Decorator command that displays a confirmation dialog before executing an inner command.
    /// Supports localized <see cref="TaskDialog"/> instances, standard <see cref="MessageBox"/> dialogs, or custom confirmation actions.
    /// </summary>
    /// <remarks>
    /// Evaluates execution eligibility against the inner command via <see cref="ICommand.CanExecute(object?)"/> and forwards
    /// <see cref="ICommand.CanExecuteChanged"/> notifications.
    /// When executed, displays the configured confirmation dialog. If the user confirms (e.g. by selecting 'Yes' or 'OK'),
    /// the inner command is invoked with the original parameter and the <see cref="Confirmed"/> event is raised.
    /// If the user declines or dismisses the dialog, the inner command is not invoked and <see cref="Cancelled"/> is raised.
    ///
    /// Typical usage with localized <see cref="ShowTaskDialogCommand"/>:
    /// <code>
    /// var deleteCommand = new ConfirmAndExecuteCommand(
    ///     innerCommand: deleteItemCommand,
    ///     heading: new LocalizableString("Delete_Heading", "Eintrag unwiderruflich löschen?"),
    ///     text: new LocalizableString("Delete_Text", "Diese Aktion kann nicht rückgängig gemacht werden."),
    ///     icon: TaskDialogIcon.Warning,
    ///     buttons: MessageBoxButtons.YesNo);
    /// </code>
    ///
    /// Typical usage wrapping an existing <see cref="ShowTaskDialogCommand"/> or <see cref="ShowMessageBoxCommand"/>:
    /// <code>
    /// var confirmCommand = new ConfirmAndExecuteCommand(innerCommand, myTaskDialogCommand);
    /// </code>
    /// </remarks>
    public class ConfirmAndExecuteCommand : ICommand, IDisposable
    {
        private readonly ICommand _innerCommand;
        private readonly ICommand? _confirmationCommand;
        private readonly Func<IWin32Window?, DialogResult> _confirmationAction;
        private readonly Func<DialogResult, bool> _isConfirmed;
        private bool _isDisposed;

        /// <summary>
        /// Gets the wrapped inner command that is executed upon confirmation.
        /// </summary>
        public ICommand InnerCommand => _innerCommand;

        /// <summary>
        /// Gets the underlying confirmation command if one was configured, or <see langword="null"/>.
        /// </summary>
        public ICommand? ConfirmationCommand => _confirmationCommand;

        /// <summary>
        /// Gets or sets an explicit owner window used for modal presentation of the confirmation dialog.
        /// </summary>
        public IWin32Window? Owner { get; set; }

        /// <summary>
        /// Gets or sets a provider function returning an owner window for the confirmation dialog.
        /// </summary>
        public Func<IWin32Window?>? OwnerProvider { get; set; }

        /// <summary>
        /// Gets the most recent <see cref="DialogResult"/> returned by the confirmation dialog.
        /// </summary>
        public DialogResult LastDialogResult { get; private set; } = DialogResult.None;

        /// <inheritdoc />
        public event EventHandler? CanExecuteChanged;

        /// <summary>
        /// Event that is raised when the confirmation dialog has been confirmed by the user.
        /// </summary>
        public event EventHandler<DialogResult>? Confirmed;

        /// <summary>
        /// Event that is raised when the confirmation dialog has been declined or cancelled by the user.
        /// </summary>
        public event EventHandler<DialogResult>? Cancelled;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfirmAndExecuteCommand"/> class wrapping a <see cref="ShowTaskDialogCommand"/>.
        /// </summary>
        /// <param name="innerCommand">The command to execute upon confirmation.</param>
        /// <param name="dialogCommand">The task dialog command used to prompt for confirmation.</param>
        /// <param name="confirmedResult">The dialog result considered as confirmation. Defaults to <see cref="DialogResult.Yes"/>.</param>
        public ConfirmAndExecuteCommand(
            ICommand innerCommand,
            ShowTaskDialogCommand dialogCommand,
            DialogResult confirmedResult = DialogResult.Yes)
            : this(
                innerCommand,
                dialogCommand,
                owner =>
                {
                    dialogCommand.Execute(owner);
                    return dialogCommand.LastDialogResult;
                },
                result => result == confirmedResult)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfirmAndExecuteCommand"/> class wrapping a <see cref="ShowMessageBoxCommand"/>.
        /// </summary>
        /// <param name="innerCommand">The command to execute upon confirmation.</param>
        /// <param name="messageBoxCommand">The message box command used to prompt for confirmation.</param>
        /// <param name="confirmedResult">The dialog result considered as confirmation. Defaults to <see cref="DialogResult.Yes"/>.</param>
        public ConfirmAndExecuteCommand(
            ICommand innerCommand,
            ShowMessageBoxCommand messageBoxCommand,
            DialogResult confirmedResult = DialogResult.Yes)
            : this(
                innerCommand,
                messageBoxCommand,
                owner =>
                {
                    messageBoxCommand.Execute(owner);
                    return messageBoxCommand.LastDialogResult;
                },
                result => result == confirmedResult)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfirmAndExecuteCommand"/> class with localized texts, creating an internal <see cref="ShowTaskDialogCommand"/>.
        /// </summary>
        /// <param name="innerCommand">The command to execute upon confirmation.</param>
        /// <param name="heading">The localized main instruction / heading text.</param>
        /// <param name="text">The optional localized descriptive text.</param>
        /// <param name="caption">The optional localized window title caption.</param>
        /// <param name="icon">The optional task dialog icon.</param>
        /// <param name="buttons">The buttons to display. Defaults to <see cref="MessageBoxButtons.YesNo"/>.</param>
        /// <param name="confirmedResult">Optional specific dialog result considered as confirmation. When <see langword="null"/>, defaults to <see cref="DialogResult.OK"/> for <see cref="MessageBoxButtons.OKCancel"/> and <see cref="DialogResult.Yes"/> otherwise.</param>
        public ConfirmAndExecuteCommand(
            ICommand innerCommand,
            LocalizableString heading,
            LocalizableString? text = null,
            LocalizableString? caption = null,
            TaskDialogIcon? icon = null,
            MessageBoxButtons buttons = MessageBoxButtons.YesNo,
            DialogResult? confirmedResult = null)
            : this(
                innerCommand,
                new ShowTaskDialogCommand(heading, text, caption, icon, buttons),
                confirmedResult ?? DefaultConfirmedResult(buttons))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfirmAndExecuteCommand"/> class with string texts, creating an internal <see cref="ShowTaskDialogCommand"/>.
        /// </summary>
        /// <param name="innerCommand">The command to execute upon confirmation.</param>
        /// <param name="heading">The main instruction / heading text.</param>
        /// <param name="text">The optional descriptive text.</param>
        /// <param name="caption">The optional window title caption.</param>
        /// <param name="icon">The optional task dialog icon.</param>
        /// <param name="buttons">The buttons to display. Defaults to <see cref="MessageBoxButtons.YesNo"/>.</param>
        /// <param name="confirmedResult">Optional specific dialog result considered as confirmation. When <see langword="null"/>, defaults to <see cref="DialogResult.OK"/> for <see cref="MessageBoxButtons.OKCancel"/> and <see cref="DialogResult.Yes"/> otherwise.</param>
        public ConfirmAndExecuteCommand(
            ICommand innerCommand,
            string heading,
            string? text = null,
            string? caption = null,
            TaskDialogIcon? icon = null,
            MessageBoxButtons buttons = MessageBoxButtons.YesNo,
            DialogResult? confirmedResult = null)
            : this(
                innerCommand,
                new ShowTaskDialogCommand(heading, text, caption, icon, buttons),
                confirmedResult ?? DefaultConfirmedResult(buttons))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfirmAndExecuteCommand"/> class with a custom <see cref="TaskDialogSetupData"/> configuration.
        /// </summary>
        /// <param name="innerCommand">The command to execute upon confirmation.</param>
        /// <param name="setupData">The setup data for the task dialog.</param>
        /// <param name="confirmedResult">Optional specific dialog result considered as confirmation.</param>
        public ConfirmAndExecuteCommand(
            ICommand innerCommand,
            TaskDialogSetupData setupData,
            DialogResult? confirmedResult = null)
            : this(
                innerCommand,
                new ShowTaskDialogCommand(setupData),
                confirmedResult ?? DefaultConfirmedResult(setupData?.Buttons ?? MessageBoxButtons.YesNo))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfirmAndExecuteCommand"/> class with a custom <see cref="MessageBoxSetupData"/> configuration.
        /// </summary>
        /// <param name="innerCommand">The command to execute upon confirmation.</param>
        /// <param name="setupData">The setup data for the message box.</param>
        /// <param name="confirmedResult">Optional specific dialog result considered as confirmation.</param>
        public ConfirmAndExecuteCommand(
            ICommand innerCommand,
            MessageBoxSetupData setupData,
            DialogResult? confirmedResult = null)
            : this(
                innerCommand,
                new ShowMessageBoxCommand(setupData),
                confirmedResult ?? DefaultConfirmedResult(setupData?.Buttons ?? MessageBoxButtons.YesNo))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfirmAndExecuteCommand"/> class with a confirmation predicate.
        /// </summary>
        /// <param name="innerCommand">The command to execute upon confirmation.</param>
        /// <param name="confirmPredicate">Function returning <see langword="true"/> when execution is confirmed; otherwise, <see langword="false"/>.</param>
        public ConfirmAndExecuteCommand(
            ICommand innerCommand,
            Func<bool> confirmPredicate)
            : this(
                innerCommand,
                confirmationCommand: null,
                _ => confirmPredicate() ? DialogResult.OK : DialogResult.Cancel,
                result => result is DialogResult.OK or DialogResult.Yes)
        {
            ArgumentNullException.ThrowIfNull(confirmPredicate);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfirmAndExecuteCommand"/> class with a functional confirmation action.
        /// </summary>
        /// <param name="innerCommand">The command to execute upon confirmation.</param>
        /// <param name="confirmationAction">The function presenting the dialog and returning a <see cref="DialogResult"/>.</param>
        /// <param name="isConfirmed">Optional predicate to check whether the resulting <see cref="DialogResult"/> represents confirmation. Defaults to <see cref="DialogResult.OK"/> or <see cref="DialogResult.Yes"/>.</param>
        public ConfirmAndExecuteCommand(
            ICommand innerCommand,
            Func<IWin32Window?, DialogResult> confirmationAction,
            Func<DialogResult, bool>? isConfirmed = null)
            : this(
                innerCommand,
                confirmationCommand: null,
                confirmationAction,
                isConfirmed ?? (result => result is DialogResult.OK or DialogResult.Yes))
        {
        }

        private ConfirmAndExecuteCommand(
            ICommand innerCommand,
            ICommand? confirmationCommand,
            Func<IWin32Window?, DialogResult> confirmationAction,
            Func<DialogResult, bool> isConfirmed)
        {
            ArgumentNullException.ThrowIfNull(innerCommand);
            ArgumentNullException.ThrowIfNull(confirmationAction);
            ArgumentNullException.ThrowIfNull(isConfirmed);

            _innerCommand = innerCommand;
            _confirmationCommand = confirmationCommand;
            _confirmationAction = confirmationAction;
            _isConfirmed = isConfirmed;

            _innerCommand.CanExecuteChanged += OnInnerCanExecuteChanged;
            _confirmationCommand?.CanExecuteChanged += OnInnerCanExecuteChanged;
        }

        /// <inheritdoc />
        public virtual bool CanExecute(object? parameter)
            => !_isDisposed && _innerCommand.CanExecute(parameter);

        /// <inheritdoc />
        public virtual void Execute(object? parameter)
        {
            if (!CanExecute(parameter))
                return;

            IWin32Window? owner = parameter as IWin32Window ?? Owner ?? OwnerProvider?.Invoke();

            DialogResult result = _confirmationAction(owner);
            LastDialogResult = result;

            if (_isConfirmed(result))
            {
                OnConfirmed(result);
                _innerCommand.Execute(parameter);
            }
            else
            {
                OnCancelled(result);
            }
        }

        /// <summary>
        /// Raises the <see cref="CanExecuteChanged"/> event.
        /// </summary>
        public void RaiseCanExecuteChanged()
            => CanExecuteChanged?.Invoke(this, EventArgs.Empty);

        /// <summary>
        /// Raises the <see cref="Confirmed"/> event.
        /// </summary>
        /// <param name="result">The dialog result confirming execution.</param>
        protected virtual void OnConfirmed(DialogResult result)
            => Confirmed?.Invoke(this, result);

        /// <summary>
        /// Raises the <see cref="Cancelled"/> event.
        /// </summary>
        /// <param name="result">The dialog result cancelling execution.</param>
        protected virtual void OnCancelled(DialogResult result)
            => Cancelled?.Invoke(this, result);

        private void OnInnerCanExecuteChanged(object? sender, EventArgs e)
            => RaiseCanExecuteChanged();

        private static DialogResult DefaultConfirmedResult(MessageBoxButtons buttons)
            => buttons switch
            {
                MessageBoxButtons.OKCancel => DialogResult.OK,
                MessageBoxButtons.RetryCancel => DialogResult.Retry,
                _ => DialogResult.Yes
            };

        /// <summary>
        /// Releases managed resources held by this command.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release managed resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed)
                return;

            if (disposing)
            {
                _innerCommand.CanExecuteChanged -= OnInnerCanExecuteChanged;
                _confirmationCommand?.CanExecuteChanged -= OnInnerCanExecuteChanged;
            }

            _isDisposed = true;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
