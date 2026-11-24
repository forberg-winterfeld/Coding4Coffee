using Coding4Coffee.WinForms.Localization.Controls;
using System.ComponentModel;
using System.Windows.Input;

namespace Coding4Coffee.WinForms.Commands.Controls
{
    /// <summary>
    /// A localizable DomainUpDown control that integrates command binding support.
    /// </summary>
    /// <remarks>
    /// Executes the bound command when the selected item changes via the <see cref="DomainUpDown.SelectedItemChanged"/> event.
    /// Inherits from <see cref="LocalizableDomainUpDown"/>, meaning dynamic localization applies to items at runtime.
    /// For each selection change, if no explicit parameter is configured, the parameter defaults to the currently selected item.
    /// Use this control for range-validated selections or dependent controls that respond to item changes.
    /// </remarks>
    public class LocalizableDomainUpDownWithCommandSupport : LocalizableDomainUpDown
    {
        private ICommand? _command;
        private object? _commandParameter;

        /// <summary>
        /// Initializes a new instance of the <see cref="LocalizableDomainUpDownWithCommandSupport"/> class.
        /// </summary>
        public LocalizableDomainUpDownWithCommandSupport()
        {
            SelectedItemChanged += DomainUpDown_SelectedItemChanged;
        }

        /// <summary>
        /// Gets or sets the command to execute when the selected item changes.
        /// </summary>
        /// <remarks>
        /// Executed in response to <see cref="DomainUpDown.SelectedItemChanged"/>.
        /// Automatically synchronizes <see cref="Control.Enabled"/> based on <see cref="ICommand.CanExecute(object?)"/>,
        /// passing <see cref="CommandParameter"/> (or <see cref="DomainUpDown.SelectedItem"/> if <see cref="CommandParameter"/> is <see langword="null"/>).
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
                }

                _command = value;

                if (_command != null)
                {
                    _command.CanExecuteChanged += Command_CanExecuteChanged;
                    Enabled = _command.CanExecute(CommandParameter ?? SelectedItem);
                }
            }
        }

        /// <summary>
        /// Gets or sets an optional command parameter. If null, SelectedItem is passed.
        /// </summary>
        /// <remarks>
        /// If this property is <see langword="null"/>, the current <see cref="DomainUpDown.SelectedItem"/>
        /// is passed as the command parameter.
        /// </remarks>
        [Browsable(true)]
        [Category("Data")]
        [Description("The parameter to pass to the command on SelectedItemChanged event. If not set, SelectedItem is used.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public object? CommandParameter
        {
            get => _commandParameter;
            set
            {
                _commandParameter = value;
                if (_command != null)
                {
                    Enabled = _command.CanExecute(_commandParameter ?? SelectedItem);
                }
            }
        }

        private void Command_CanExecuteChanged(object? sender, EventArgs e)
            => Enabled = _command?.CanExecute(CommandParameter ?? SelectedItem) ?? true;

        private void DomainUpDown_SelectedItemChanged(object? sender, EventArgs e)
        {
            if (_command != null)
            {
                var parameter = CommandParameter ?? SelectedItem;
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
        /// <returns>The current <see cref="LocalizableDomainUpDownWithCommandSupport"/> instance to support fluent method chaining.</returns>
        public LocalizableDomainUpDownWithCommandSupport BindCommand(ICommand command, object? parameter = null)
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
