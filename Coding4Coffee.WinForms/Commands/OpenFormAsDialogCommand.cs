using Coding4Coffee.WinForms.Commands.CommandEventArgs;
using System.Windows.Input;

namespace Coding4Coffee.WinForms.Commands
{
    /// <summary>
    /// Command to open a Windows Form as a modal dialog.
    /// Manages the form's execution, result capture, and cleanup.
    /// </summary>
    /// <remarks>
    /// Opens a form modally using <see cref="Form.ShowDialog(IWin32Window)"/>, blocking execution until the form closes.
    /// The result is available via <see cref="LastDialogResult"/>.
    /// Optionally, subscribers can listen to the <see cref="DialogClosed"/> event to be notified when the dialog closes.
    ///
    /// Typical usage (imperative):
    /// <code>
    /// var openSettingsCommand = new OpenFormAsDialogCommand&lt;SettingsForm&gt;();
    /// openSettingsCommand.Execute(ownerForm);
    /// if (openSettingsCommand.LastDialogResult == DialogResult.OK)
    ///     ApplySettings();
    /// </code>
    ///
    /// Typical usage (reactive with event):
    /// <code>
    /// var openSettingsCommand = new OpenFormAsDialogCommand&lt;SettingsForm&gt;();
    /// openSettingsCommand.DialogClosed += (_, e) =&gt; 
    /// {
    ///     if (e.DialogResult == DialogResult.OK)
    ///         ApplySettings();
    /// };
    /// openSettingsCommand.Execute(ownerForm);
    /// </code>
    /// </remarks>
    /// <typeparam name="TForm">The type of the Form to open as a modal dialog.</typeparam>
    public class OpenFormAsDialogCommand<TForm> : ICommand
        where TForm : Form
    {
        private readonly Func<TForm> _createForm;
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
        public event EventHandler<FormClosedEventArgs<TForm>>? DialogClosed;

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenFormAsDialogCommand{TForm}"/> class.
        /// </summary>
        /// <remarks>
        /// Instantiates <typeparamref name="TForm"/> using <see cref="Activator.CreateInstance{TForm}"/>.
        /// <typeparamref name="TForm"/> must have a public parameterless constructor.
        /// </remarks>
        public OpenFormAsDialogCommand()
            : this(Activator.CreateInstance<TForm>, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenFormAsDialogCommand{TForm}"/> class using a factory function.
        /// </summary>
        /// <param name="createForm">The factory function used to instantiate the form.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="createForm"/> is <see langword="null"/>.</exception>
        public OpenFormAsDialogCommand(Func<TForm> createForm)
            : this(createForm, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenFormAsDialogCommand{TForm}"/> class with full configuration.
        /// </summary>
        /// <param name="createForm">The factory function used to instantiate the form.</param>
        /// <param name="canExecute">Optional function that evaluates the parameter to determine whether the command can execute.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="createForm"/> is <see langword="null"/>.</exception>
        public OpenFormAsDialogCommand(Func<TForm> createForm, Func<object?, bool>? canExecute)
        {
            ArgumentNullException.ThrowIfNull(createForm);
            _createForm = createForm;
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
        /// Executes the command, opening the form as a modal dialog.
        /// </summary>
        /// <remarks>
        /// Creates a new form instance and shows it modally using <see cref="Form.ShowDialog(IWin32Window)"/>.
        /// If <paramref name="parameter"/> is an <see cref="IWin32Window"/>, it is used as the owner for the dialog.
        /// The method blocks until the form closes, captures the <see cref="Form.DialogResult"/>,
        /// raises the optional close event, and disposes the form.
        /// </remarks>
        /// <param name="parameter">An optional owner window to make the dialog modal relative to it.</param>
        public virtual void Execute(object? parameter)
        {
            if (!CanExecute(parameter))
                return;

            var form = _createForm();

            try
            {
                LastDialogResult = parameter is IWin32Window owner ? form.ShowDialog(owner) : form.ShowDialog();

                OnDialogClosed(new FormClosedEventArgs<TForm>(form, LastDialogResult));
            }
            finally
            {
                form.Dispose();
            }
        }

        /// <summary>
        /// Raises the <see cref="DialogClosed"/> event.
        /// </summary>
        protected virtual void OnDialogClosed(FormClosedEventArgs<TForm> e)
            => DialogClosed?.Invoke(this, e);

        /// <summary>
        /// Raises the <see cref="CanExecuteChanged"/> event.
        /// </summary>
        public virtual void RaiseCanExecuteChanged()
            => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
