using Coding4Coffee.WinForms.Localization.Controls;
using System.ComponentModel;
using System.Windows.Input;

namespace Coding4Coffee.WinForms.Commands.Controls
{
    /// <summary>
    /// A localizable ToolStripComboBox control that integrates command binding support.
    /// Executes the bound command when the selection is committed by the user.
    /// </summary>
    public class LocalizableToolStripComboBoxWithCommandSupport : LocalizableToolStripComboBox
    {
        private ICommand? _command;
        private object? _commandParameter;

        /// <summary>
        /// Gets or sets the command to execute when the combo box selection changes.
        /// </summary>
        /// <remarks>
        /// Shadows the inherited <see cref="ToolStripItem.Command"/> property intentionally.
        /// The command is invoked in response to <see cref="ComboBox.SelectionChangeCommitted"/>,
        /// meaning it executes only when the user changes the selection in the UI, not when
        /// <see cref="ComboBox.SelectedIndex"/> or <see cref="ComboBox.SelectedItem"/> is changed programmatically.
        /// Automatically synchronizes <see cref="ToolStripItem.Enabled"/> based on <see cref="ICommand.CanExecute(object?)"/>,
        /// passing <see cref="CommandParameter"/> (or <see cref="ComboBox.SelectedItem"/> if <see cref="CommandParameter"/> is <see langword="null"/>).
        /// </remarks>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public new ICommand? Command
        {
            get => _command;
            set
            {
                if (_command != null)
                {
                    _command.CanExecuteChanged -= Command_CanExecuteChanged;
                    ComboBox.SelectionChangeCommitted -= ComboBox_SelectionChangeCommitted;
                }

                _command = value;

                if (_command != null)
                {
                    _command.CanExecuteChanged += Command_CanExecuteChanged;
                    ComboBox.SelectionChangeCommitted += ComboBox_SelectionChangeCommitted;

                    Enabled = _command.CanExecute(CommandParameter ?? ComboBox.SelectedItem);
                }
            }
        }

        /// <summary>
        /// Gets or sets an optional command parameter. If null, SelectedItem is passed.
        /// </summary>
        /// <remarks>
        /// Shadows the inherited <see cref="ToolStripItem.CommandParameter"/> property intentionally.
        /// If this property is <see langword="null"/>, the control passes its current <see cref="ComboBox.SelectedItem"/>
        /// as the command parameter.
        /// </remarks>
        [Browsable(true)]
        [Category("Data")]
        [Description("The parameter to pass to the command on SelectionChangeCommitted event. If not set, SelectedItem is used.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public new object? CommandParameter
        {
            get => _commandParameter;
            set
            {
                _commandParameter = value;
                if (_command != null)
                {
                    Enabled = _command.CanExecute(_commandParameter ?? ComboBox.SelectedItem);
                }
            }
        }

        private void Command_CanExecuteChanged(object? sender, EventArgs e)
            => Enabled = _command?.CanExecute(CommandParameter ?? ComboBox.SelectedItem) ?? true;

        private void ComboBox_SelectionChangeCommitted(object? sender, EventArgs e)
        {
            if (_command != null)
            {
                var parameter = CommandParameter ?? ComboBox.SelectedItem;
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
        /// <returns>The current <see cref="LocalizableToolStripComboBoxWithCommandSupport"/> instance to support fluent method chaining.</returns>
        public LocalizableToolStripComboBoxWithCommandSupport BindCommand(ICommand command, object? parameter = null)
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
