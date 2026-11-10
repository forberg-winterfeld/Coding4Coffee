using System.Windows.Input;

namespace Coding4Coffee.WinForms.Commands
{
    /// <summary>
    /// Abstract base class for commands that manipulate a <see cref="Form"/>'s <see cref="FormWindowState"/>.
    /// </summary>
    /// <remarks>
    /// Supports binding to a fixed form instance, resolving via a delegate provider, or obtaining from the command parameter.
    /// When bound to a fixed form, automatically hooks into <see cref="Control.Resize"/> and <see cref="System.ComponentModel.Component.Disposed"/>
    /// to trigger <see cref="CanExecuteChanged"/> and clean up event subscriptions.
    /// </remarks>
    public abstract class FormWindowStateCommandBase : ICommand, IDisposable
    {
        private readonly Form? _fixedForm;
        private readonly Func<Form?>? _formProvider;
        private bool _isDisposed;

        /// <inheritdoc />
        public event EventHandler? CanExecuteChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="FormWindowStateCommandBase"/> class bound to a specific form.
        /// </summary>
        /// <param name="form">The target form.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="form"/> is <see langword="null"/>.</exception>
        protected FormWindowStateCommandBase(Form form)
        {
            ArgumentNullException.ThrowIfNull(form);
            _fixedForm = form;
            _fixedForm.Resize += OnFormStateChanged;
            _fixedForm.Disposed += OnFormDisposed;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FormWindowStateCommandBase"/> class that resolves the form dynamically.
        /// </summary>
        /// <param name="formProvider">Optional delegate returning the target form.</param>
        protected FormWindowStateCommandBase(Func<Form?>? formProvider = null)
        {
            _formProvider = formProvider;
        }

        /// <summary>
        /// Resolves the target form from the fixed instance, the execution parameter, the provider, or <see cref="Form.ActiveForm"/>.
        /// </summary>
        /// <param name="parameter">The optional command parameter.</param>
        /// <returns>The resolved <see cref="Form"/>, or <see langword="null"/> if none can be found.</returns>
        protected virtual Form? ResolveForm(object? parameter)
            => _fixedForm ?? parameter as Form ?? _formProvider?.Invoke() ?? Form.ActiveForm;

        /// <inheritdoc />
        public virtual bool CanExecute(object? parameter)
        {
            if (_isDisposed)
                return false;

            Form? form = ResolveForm(parameter);
            if (form == null || form.IsDisposed || form.Disposing)
                return false;

            return CanExecuteForForm(form, parameter);
        }

        /// <inheritdoc />
        public virtual void Execute(object? parameter)
        {
            if (!CanExecute(parameter))
                return;

            Form? form = ResolveForm(parameter);
            if (form == null || form.IsDisposed || form.Disposing)
                return;

            ExecuteForForm(form, parameter);
            RaiseCanExecuteChanged();
        }

        /// <summary>
        /// Determines whether the command can execute for the specified valid form.
        /// </summary>
        /// <param name="form">The non-disposed target form.</param>
        /// <param name="parameter">The command parameter.</param>
        /// <returns><see langword="true"/> if execution is allowed; otherwise, <see langword="false"/>.</returns>
        protected abstract bool CanExecuteForForm(Form form, object? parameter);

        /// <summary>
        /// Executes the window state transition on the specified form.
        /// </summary>
        /// <param name="form">The target form.</param>
        /// <param name="parameter">The command parameter.</param>
        protected abstract void ExecuteForForm(Form form, object? parameter);

        /// <summary>
        /// Raises the <see cref="CanExecuteChanged"/> event.
        /// </summary>
        public void RaiseCanExecuteChanged()
            => CanExecuteChanged?.Invoke(this, EventArgs.Empty);

        private void OnFormStateChanged(object? sender, EventArgs e)
            => RaiseCanExecuteChanged();

        private void OnFormDisposed(object? sender, EventArgs e)
            => Dispose();

        /// <summary>
        /// Releases managed resources held by this command.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release managed resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed)
                return;

            if (disposing && _fixedForm != null)
            {
                _fixedForm.Resize -= OnFormStateChanged;
                _fixedForm.Disposed -= OnFormDisposed;
            }

            _isDisposed = true;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
