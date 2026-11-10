using Coding4Coffee.WinForms.Commands.Enums;
using System.Windows.Input;

namespace Coding4Coffee.WinForms.Commands
{
    /// <summary>
    /// Command to switch the selected tab of a <see cref="TabControl"/> forward or backward.
    /// </summary>
    public class SwitchTabCommand : ICommand, IDisposable
    {
        private readonly TabControl _tabControl;
        private bool _isDisposed;

        /// <param name="tabControl">The tab control whose tabs are to be switched.</param>
        public SwitchTabCommand(TabControl tabControl)
        {
            ArgumentNullException.ThrowIfNull(tabControl);

            _tabControl = tabControl;
            _tabControl.SelectedIndexChanged += OnTabControlStateChanged;
            _tabControl.ControlAdded += OnTabControlStateChanged;
            _tabControl.ControlRemoved += OnTabControlStateChanged;
            _tabControl.Disposed += OnTabControlDisposed;
        }

        /// <inheritdoc />
        public event EventHandler? CanExecuteChanged;

        /// <inheritdoc />
        public bool CanExecute(object? parameter)
        {
            if (_isDisposed
                || parameter is not SwitchTabDirection direction
                || _tabControl.IsDisposed
                || _tabControl.TabCount == 0
                || _tabControl.SelectedIndex < 0)
            {
                return false;
            }

            return direction switch
            {
                SwitchTabDirection.Next => _tabControl.SelectedIndex < _tabControl.TabCount - 1,
                SwitchTabDirection.Previous => _tabControl.SelectedIndex > 0,
                _ => false
            };
        }

        /// <inheritdoc />
        public void Execute(object? parameter)
        {
            if (!CanExecute(parameter) || parameter is not SwitchTabDirection direction)
                return;

            switch (direction)
            {
                case SwitchTabDirection.Next:
                    _tabControl.SelectedIndex++;
                    break;
                case SwitchTabDirection.Previous:
                    _tabControl.SelectedIndex--;
                    break;
            }

            RaiseCanExecuteChanged();
        }

        /// <summary>
        /// Raises the <see cref="CanExecuteChanged"/> event.
        /// </summary>
        public void RaiseCanExecuteChanged() 
            => CanExecuteChanged?.Invoke(this, EventArgs.Empty);

        private void OnTabControlStateChanged(object? _, EventArgs __) 
            => RaiseCanExecuteChanged();

        private void OnTabControlDisposed(object? _, EventArgs __) 
            => Dispose();

        /// <summary>
        /// Releases managed and unmanaged resources.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release managed resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed)
                return;

            if (disposing)
            {
                _tabControl.SelectedIndexChanged -= OnTabControlStateChanged;
                _tabControl.ControlAdded -= OnTabControlStateChanged;
                _tabControl.ControlRemoved -= OnTabControlStateChanged;
                _tabControl.Disposed -= OnTabControlDisposed;
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
