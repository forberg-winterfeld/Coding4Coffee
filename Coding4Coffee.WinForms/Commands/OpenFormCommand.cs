using System.Windows.Input;

namespace Coding4Coffee.WinForms.Commands
{
    /// <summary>
    /// Command to open a Windows Form of a specific type.
    /// Manages the lifetime of the form, showing it as a modeless window,
    /// and bringing it to focus/restoring it if it is already open.
    /// </summary>
    /// <remarks>
    /// Maintains a single managed window instance of type <typeparamref name="TForm"/>.
    /// If the form is not currently open, executing the command creates and displays it via <see cref="Form.Show"/>.
    /// If the form is already open, executing the command does not create a new window; instead, it restores the
    /// window from a minimized state (setting <see cref="Form.WindowState"/> to <see cref="FormWindowState.Normal"/>),
    /// brings it to front, and activates it.
    /// When the form is closed by the user, <see cref="ManagedForm"/> is reset to <see langword="null"/> and <see cref="CanExecuteChanged"/> is raised.
    /// </remarks>
    /// <typeparam name="TForm">The type of the Form to open.</typeparam>
    public class OpenFormCommand<TForm> : ICommand
        where TForm : Form
    {
        private readonly Func<TForm> _createForm;
        private readonly Func<bool>? _canExecute;

        /// <summary>
        /// Gets the instance of the form managed by this command, or <see langword="null"/> if not currently open.
        /// </summary>
        public TForm? ManagedForm { get; protected set; }

        /// <summary>
        /// Gets a value indicating whether the form is currently open.
        /// </summary>
        public bool IsOpen => ManagedForm != null;

        /// <summary>
        /// Event that is raised when the command's ability to execute changes.
        /// </summary>
        public event EventHandler? CanExecuteChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenFormCommand{TForm}"/> class using the parameterless constructor of <typeparamref name="TForm"/>.
        /// </summary>
        /// <remarks>
        /// Instantiates <typeparamref name="TForm"/> using <see cref="Activator.CreateInstance{TForm}"/>.
        /// <typeparamref name="TForm"/> must have a public parameterless constructor.
        /// </remarks>
        public OpenFormCommand()
            : this(Activator.CreateInstance<TForm>, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenFormCommand{TForm}"/> class using a factory function.
        /// </summary>
        /// <param name="createForm">The factory function used to instantiate the form.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="createForm"/> is <see langword="null"/>.</exception>
        public OpenFormCommand(Func<TForm> createForm)
            : this(createForm, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenFormCommand{TForm}"/> class using an execution predicate and the parameterless constructor of <typeparamref name="TForm"/>.
        /// </summary>
        /// <remarks>
        /// Instantiates <typeparamref name="TForm"/> using <see cref="Activator.CreateInstance{TForm}"/>.
        /// <typeparamref name="TForm"/> must have a public parameterless constructor.
        /// </remarks>
        /// <param name="canExecute">The parameterless function that determines whether the command can execute.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="canExecute"/> is <see langword="null"/>.</exception>
        public OpenFormCommand(Func<bool> canExecute)
            : this(Activator.CreateInstance<TForm>, canExecute)
        {
            ArgumentNullException.ThrowIfNull(canExecute);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenFormCommand{TForm}"/> class using a factory function and an optional execution predicate.
        /// </summary>
        /// <param name="createForm">The factory function used to instantiate the form.</param>
        /// <param name="canExecute">The optional parameterless function that determines whether the command can execute.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="createForm"/> is <see langword="null"/>.</exception>
        public OpenFormCommand(Func<TForm> createForm, Func<bool>? canExecute)
        {
            ArgumentNullException.ThrowIfNull(createForm);
            _createForm = createForm;
            _canExecute = canExecute;
        }

        /// <summary>
        /// Returns whether the command can execute in its current state.
        /// </summary>
        /// <param name="parameter">An optional parameter (ignored by the execution predicate).</param>
        /// <returns><see langword="true"/> if the command can execute; otherwise, <see langword="false"/>.</returns>
        public virtual bool CanExecute(object? parameter)
            => _canExecute?.Invoke() ?? true;

        /// <summary>
        /// Executes the command, opening the form or restoring and focusing it if it is already open.
        /// </summary>
        /// <remarks>
        /// If <paramref name="parameter"/> is a <see cref="Form"/> whose <see cref="Form.IsMdiContainer"/> is <see langword="true"/>,
        /// the newly instantiated form will be configured as an MDI child by setting its <see cref="Form.MdiParent"/> property.
        /// If the form is already open, it is restored if minimized, brought to the foreground, and focused.
        /// </remarks>
        /// <param name="parameter">An optional MDI parent form if the managed form should be an MDI child.</param>
        public virtual void Execute(object? parameter)
        {
            if (!CanExecute(parameter))
                return;

            if (ManagedForm == null)
            {
                ManagedForm = _createForm();
                ManagedForm.FormClosed += ManagedForm_FormClosed;

                if (parameter is Form mdiParent && mdiParent.IsMdiContainer)
                    ManagedForm.MdiParent = mdiParent;

                ManagedForm.Show();

                OnCanExecuteChanged();
            }
            else
            {
                if (ManagedForm.WindowState == FormWindowState.Minimized)
                    ManagedForm.WindowState = FormWindowState.Normal;

                ManagedForm.BringToFront();
                ManagedForm.Activate();
            }
        }

        /// <summary>
        /// Raises the <see cref="CanExecuteChanged"/> event.
        /// </summary>
        protected virtual void OnCanExecuteChanged()
            => CanExecuteChanged?.Invoke(this, EventArgs.Empty);

        private void ManagedForm_FormClosed(object? sender, FormClosedEventArgs e)
        {
            if (sender is Form form)
                form.FormClosed -= ManagedForm_FormClosed;

            ManagedForm = null;
            OnCanExecuteChanged();
        }
    }
}
