using System.Windows.Input;

namespace Coding4Coffee.WinForms.Commands
{
    /// <summary>
    /// Command to set keyboard input focus to a specified <see cref="Control"/>.
    /// </summary>
    /// <remarks>
    /// Activates focus on the target control via <see cref="Control.Focus()"/> or <see cref="Control.Select()"/>.
    /// If the target is a <see cref="TextBoxBase"/> and <see cref="SelectAllOnFocus"/> is enabled, all text is automatically selected.
    /// Supports binding to a fixed control, resolving via a delegate provider, or obtaining the control from the command parameter.
    ///
    /// Typical usage (shortcut to focus search box):
    /// <code>
    /// var focusSearchCommand = new FocusControlCommand(searchTextBox) { SelectAllOnFocus = true };
    /// findMenuItem.BindCommand(focusSearchCommand);
    /// </code>
    /// </remarks>
    public class FocusControlCommand : ICommand, IDisposable
    {
        private readonly Control? _fixedControl;
        private readonly Func<Control?>? _controlProvider;
        private readonly Func<object?, bool>? _canExecute;
        private bool _isDisposed;

        /// <summary>
        /// Gets or sets a value indicating whether all text in a <see cref="TextBoxBase"/> should be selected upon receiving focus.
        /// Defaults to <see langword="false"/>.
        /// </summary>
        public bool SelectAllOnFocus { get; init; }

        /// <inheritdoc />
        public event EventHandler? CanExecuteChanged;

        /// <summary>
        /// Event that is raised after a control has successfully received focus.
        /// </summary>
        public event EventHandler<Control>? Focused;

        /// <summary>
        /// Initializes a new instance of the <see cref="FocusControlCommand"/> class bound to the specified control.
        /// </summary>
        /// <param name="control">The target control to receive focus.</param>
        /// <param name="canExecute">Optional predicate to determine whether the command can execute.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="control"/> is <see langword="null"/>.</exception>
        public FocusControlCommand(Control control, Func<object?, bool>? canExecute = null)
        {
            ArgumentNullException.ThrowIfNull(control);
            _fixedControl = control;
            _canExecute = canExecute;

            _fixedControl.EnabledChanged += OnControlStateChanged;
            _fixedControl.VisibleChanged += OnControlStateChanged;
            _fixedControl.Disposed += OnControlDisposed;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FocusControlCommand"/> class that resolves the control via a provider or command parameter.
        /// </summary>
        /// <param name="controlProvider">Optional delegate returning the target control.</param>
        /// <param name="canExecute">Optional predicate to determine whether the command can execute.</param>
        public FocusControlCommand(Func<Control?>? controlProvider = null, Func<object?, bool>? canExecute = null)
        {
            _controlProvider = controlProvider;
            _canExecute = canExecute;
        }

        /// <inheritdoc />
        public virtual bool CanExecute(object? parameter)
        {
            if (_isDisposed)
                return false;

            Control? control = ResolveControl(parameter);
            if (control == null || control.IsDisposed || control.Disposing)
                return false;

            if (!control.Enabled || !control.Visible || !control.CanFocus)
                return false;

            return _canExecute?.Invoke(parameter) ?? true;
        }

        /// <inheritdoc />
        public virtual void Execute(object? parameter)
        {
            if (!CanExecute(parameter))
                return;

            Control? control = ResolveControl(parameter);
            if (control == null || control.IsDisposed || control.Disposing)
                return;

            control.Focus();

            if (SelectAllOnFocus && control is TextBoxBase textBox)
                textBox.SelectAll();

            OnFocused(control);
        }

        /// <summary>
        /// Raises the <see cref="CanExecuteChanged"/> event.
        /// </summary>
        public void RaiseCanExecuteChanged()
            => CanExecuteChanged?.Invoke(this, EventArgs.Empty);

        /// <summary>
        /// Raises the <see cref="Focused"/> event.
        /// </summary>
        /// <param name="control">The control that received focus.</param>
        protected virtual void OnFocused(Control control)
            => Focused?.Invoke(this, control);

        /// <summary>
        /// Resolves the target control from the fixed instance, the execution parameter, or the provider.
        /// </summary>
        /// <param name="parameter">The optional command parameter.</param>
        /// <returns>The resolved <see cref="Control"/>, or <see langword="null"/> if none can be found.</returns>
        protected virtual Control? ResolveControl(object? parameter)
            => _fixedControl ?? parameter as Control ?? _controlProvider?.Invoke();

        private void OnControlStateChanged(object? _, EventArgs __)
            => RaiseCanExecuteChanged();

        private void OnControlDisposed(object? _, EventArgs __)
            => Dispose();

        /// <summary>
        /// Releases managed resources held by this command.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release managed resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed)
                return;

            if (disposing && _fixedControl != null)
            {
                _fixedControl.EnabledChanged -= OnControlStateChanged;
                _fixedControl.VisibleChanged -= OnControlStateChanged;
                _fixedControl.Disposed -= OnControlDisposed;
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
