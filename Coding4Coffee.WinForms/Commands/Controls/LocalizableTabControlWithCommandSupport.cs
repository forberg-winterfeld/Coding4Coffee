using Coding4Coffee.WinForms.Localization.Controls;
using System.ComponentModel;
using System.Windows.Input;

namespace Coding4Coffee.WinForms.Commands.Controls
{
    /// <summary>
    /// A localizable TabControl that integrates command binding support.
    /// Executes the bound command when the selected tab changes.
    /// </summary>
    public class LocalizableTabControlWithCommandSupport : LocalizableTabControl
    {
        private ICommand? _command;
        private object? _commandParameter;

        /// <summary>
        /// Gets or sets the command to execute when the selected tab changes.
        /// </summary>
        /// <remarks>
        /// Executes the command whenever <see cref="TabControl.SelectedIndexChanged"/> is raised.
        /// Automatically synchronizes <see cref="Control.Enabled"/> based on <see cref="ICommand.CanExecute(object?)"/>,
        /// passing <see cref="CommandParameter"/> (or the default parameter determined by <see cref="TabControl.SelectedTab"/>).
        /// </remarks>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public ICommand? Command
        {
            get => _command;
            set
            {
                if (_command != null)
                {
                    _command.CanExecuteChanged -= Command_CanExecuteChanged;
                    SelectedIndexChanged -= TabControl_SelectedIndexChanged;
                }

                _command = value;

                if (_command != null)
                {
                    _command.CanExecuteChanged += Command_CanExecuteChanged;
                    SelectedIndexChanged += TabControl_SelectedIndexChanged;

                    Enabled = _command.CanExecute(CommandParameter ?? GetDefaultParameter());
                }
            }
        }

        /// <summary>
        /// Gets or sets an optional command parameter. If null, SelectedTab.Name (or SelectedTab) is passed.
        /// </summary>
        /// <remarks>
        /// If this property is <see langword="null"/>, the parameter passed to the command defaults to
        /// <see cref="TabControl.SelectedTab"/>'s <see cref="Control.Name"/> (if non-null), the <see cref="TabPage"/>
        /// instance itself, or the integer <see cref="TabControl.SelectedIndex"/> if no tab page is selected.
        /// </remarks>
        [Browsable(true)]
        [Category("Data")]
        [Description("The parameter to pass to the command on SelectedIndexChanged event. If not set, SelectedTab.Name is used.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public object? CommandParameter
        {
            get => _commandParameter;
            set
            {
                _commandParameter = value;
                if (_command != null)
                {
                    Enabled = _command.CanExecute(_commandParameter ?? GetDefaultParameter());
                }
            }
        }

        private object GetDefaultParameter() => SelectedTab?.Name ?? SelectedTab ?? (object)SelectedIndex;

        private void Command_CanExecuteChanged(object? sender, EventArgs e)
            => Enabled = _command?.CanExecute(CommandParameter ?? GetDefaultParameter()) ?? true;

        private void TabControl_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (_command != null)
            {
                var parameter = CommandParameter ?? GetDefaultParameter();
                if (_command.CanExecute(parameter))
                {
                    _command.Execute(parameter);
                }
            }
        }

        /// <summary>
        /// Binds a command and optional parameter to the control.
        /// </summary>
        /// <param name="command">The command to assign.</param>
        /// <param name="parameter">The parameter value for the command.</param>
        /// <returns>The current <see cref="LocalizableTabControlWithCommandSupport"/> instance to support fluent method chaining.</returns>
        public LocalizableTabControlWithCommandSupport BindCommand(ICommand command, object? parameter = null)
        {
            CommandParameter = parameter;
            Command = command;

            return this;
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
                Command = null;
            base.Dispose(disposing);
        }
    }
}
