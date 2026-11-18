using Coding4Coffee.WinForms.Localization.Controls;
using System.ComponentModel;
using System.Windows.Input;

namespace Coding4Coffee.WinForms.Commands.Controls
{
    /// <summary>
    /// A localizable ListBox control that integrates command binding support.
    /// Executes the bound command when the selected item changes.
    /// </summary>
    public class LocalizableListBoxWithCommandSupport : LocalizableListBox
    {
        private ICommand? _command;
        private object? _commandParameter;

        /// <summary>
        /// Gets or sets the command to execute when the selection changes.
        /// </summary>
        /// <remarks>
        /// The command is invoked in response to <see cref="ListBox.SelectedIndexChanged"/>.
        /// Automatically synchronizes <see cref="Control.Enabled"/> based on <see cref="ICommand.CanExecute(object?)"/>,
        /// passing <see cref="CommandParameter"/> (or <see cref="ListBox.SelectedItem"/> if <see cref="CommandParameter"/> is <see langword="null"/>).
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
                    SelectedIndexChanged -= ListBox_SelectedIndexChanged;
                }

                _command = value;

                if (_command != null)
                {
                    _command.CanExecuteChanged += Command_CanExecuteChanged;
                    SelectedIndexChanged += ListBox_SelectedIndexChanged;

                    Enabled = _command.CanExecute(CommandParameter ?? SelectedItem);
                }
            }
        }

        /// <summary>
        /// Gets or sets an optional command parameter. If null, <see cref="ListBox.SelectedItem"/> is passed.
        /// </summary>
        /// <remarks>
        /// If this property is <see langword="null"/>, the control passes its current <see cref="ListBox.SelectedItem"/>
        /// as the command parameter.
        /// </remarks>
        [Browsable(true)]
        [Category("Data")]
        [Description("The parameter to pass to the command on SelectedIndexChanged event. If not set, SelectedItem is used.")]
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

        private void ListBox_SelectedIndexChanged(object? sender, EventArgs e)
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
        /// <returns>The current <see cref="LocalizableListBoxWithCommandSupport"/> instance to support fluent method chaining.</returns>
        public LocalizableListBoxWithCommandSupport BindCommand(ICommand command, object? parameter = null)
        {
            CommandParameter = parameter;
            Command = command;

            return this;
        }
    }
}
