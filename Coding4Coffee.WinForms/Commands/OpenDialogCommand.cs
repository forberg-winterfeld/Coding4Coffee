using Coding4Coffee.WinForms.Commands.CommandEventArgs;
using System.Windows.Input;

namespace Coding4Coffee.WinForms.Commands
{
    /// <summary>
    /// Command to open a Windows File/Folder Dialog of a specific type.
    /// Manages the dialog's execution, configuration, result capture, and cleanup.
    /// </summary>
    /// <remarks>
    /// Opens a dialog (e.g., <see cref="OpenFileDialog"/>, <see cref="SaveFileDialog"/>, <see cref="FolderBrowserDialog"/>)
    /// modally and captures the result. The dialog can be configured before execution through an optional setup action.
    /// After the dialog closes, the result is available via <see cref="LastDialogResult"/>.
    /// Optionally, subscribers can listen to the <see cref="DialogClosed"/> event to be notified when the dialog closes.
    /// </remarks>
    /// <typeparam name="TDialog">The type of the dialog to open (e.g., <see cref="OpenFileDialog"/>, <see cref="FolderBrowserDialog"/>).</typeparam>
    public class OpenDialogCommand<TDialog> : ICommand
        where TDialog : CommonDialog
    {
        private readonly Func<TDialog> _createDialog;
        private readonly Action<TDialog>? _setupDialog;
        private readonly Func<object?, bool>? _canExecute;

        /// <summary>
        /// Gets the most recent dialog result from the last execution.
        /// </summary>
        public DialogResult LastDialogResult { get; private set; }

        /// <summary>
        /// Event that is raised when the command's ability to execute changes.
        /// </summary>
        public event EventHandler? CanExecuteChanged;

        /// <summary>
        /// Event that is raised when a dialog closes after execution.
        /// </summary>
        public event EventHandler<DialogClosedEventArgs<TDialog>>? DialogClosed;

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenDialogCommand{TDialog}"/> class with a setup action.
        /// </summary>
        /// <param name="setupDialog">Optional action to configure the dialog before it is shown.</param>
        public OpenDialogCommand(Action<TDialog>? setupDialog = null)
            : this(Activator.CreateInstance<TDialog>, setupDialog, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenDialogCommand{TDialog}"/> class with a factory and setup action.
        /// </summary>
        /// <param name="createDialog">The factory function used to instantiate the dialog.</param>
        /// <param name="setupDialog">Optional action to configure the dialog before it is shown.</param>
        public OpenDialogCommand(Func<TDialog> createDialog, Action<TDialog>? setupDialog)
            : this(createDialog, setupDialog, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenDialogCommand{TDialog}"/> class with full configuration.
        /// </summary>
        /// <param name="createDialog">The factory function used to instantiate the dialog.</param>
        /// <param name="setupDialog">Optional action to configure the dialog before it is shown.</param>
        /// <param name="canExecute">Optional function that evaluates the parameter to determine whether the command can execute.</param>
        public OpenDialogCommand(Func<TDialog> createDialog, Action<TDialog>? setupDialog, Func<object?, bool>? canExecute)
        {
            ArgumentNullException.ThrowIfNull(createDialog);
            _createDialog = createDialog;
            _setupDialog = setupDialog;
            _canExecute = canExecute;
        }

        /// <summary>
        /// Returns whether the command can execute in its current state.
        /// </summary>
        /// <param name="parameter">An optional owner window parameter for dialog modality or custom execution logic.</param>
        /// <returns><see langword="true"/> if the command can execute; otherwise, <see langword="false"/>.</returns>
        public virtual bool CanExecute(object? parameter)
            => _canExecute?.Invoke(parameter) ?? true;

        /// <summary>
        /// Executes the command, opening the dialog and capturing the result.
        /// </summary>
        /// <param name="parameter">An optional owner window to make the dialog modal relative to it.</param>
        public virtual void Execute(object? parameter)
        {
            if (!CanExecute(parameter))
                return;

            TDialog dialog = _createDialog();

            try
            {
                _setupDialog?.Invoke(dialog);

                LastDialogResult = parameter is IWin32Window owner ? dialog.ShowDialog(owner) : dialog.ShowDialog();

                OnDialogClosed(new DialogClosedEventArgs<TDialog>(dialog, LastDialogResult));
            }
            finally
            {
                dialog?.Dispose();
            }
        }

        /// <summary>
        /// Raises the <see cref="DialogClosed"/> event.
        /// </summary>
        protected virtual void OnDialogClosed(DialogClosedEventArgs<TDialog> e)
            => DialogClosed?.Invoke(this, e);

        /// <summary>
        /// Raises the <see cref="CanExecuteChanged"/> event.
        /// </summary>
        public virtual void RaiseCanExecuteChanged() 
            => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
