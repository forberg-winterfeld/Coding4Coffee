using Coding4Coffee.WinForms.Localization.Controls;
using System.ComponentModel;
using System.Windows.Input;

namespace Coding4Coffee.WinForms.Commands.Controls
{
    /// <summary>
    /// A localizable CheckBox control that integrates command binding support.
    /// Executes the bound command when the checked state changes.
    /// </summary>
    public class LocalizableCheckBoxWithCommandSupport : LocalizableCheckBox
    {
        private ICommand? _command;
        private object? _commandParameter;

        /// <summary>
        /// Gets or sets the command to execute when the checked state changes.
        /// </summary>
        /// <remarks>
        /// Shadows <see cref="ButtonBase.Command"/> intentionally. The inherited property
        /// executes the command on <c>Click</c>, whereas this implementation executes it on
        /// <c>CheckedChanged</c> – the semantically correct trigger for a toggle control.
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
                    CheckedChanged -= CheckBox_CheckedChanged;
                }

                _command = value;

                if (_command != null)
                {
                    _command.CanExecuteChanged += Command_CanExecuteChanged;
                    CheckedChanged += CheckBox_CheckedChanged;

                    Enabled = _command.CanExecute(CommandParameter ?? Checked);
                }
            }
        }

        /// <summary>
        /// Gets or sets an optional command parameter. If null, <see cref="CheckBox.Checked"/> is passed.
        /// </summary>
        /// <remarks>
        /// Shadows <see cref="ButtonBase.CommandParameter"/> to stay consistent with the
        /// shadowed <see cref="Command"/> property and ensure the correct parameter is
        /// forwarded when the command fires on <c>CheckedChanged</c>.
        /// If set to <see langword="null"/>, the current <see cref="CheckBox.Checked"/> value
        /// (<see cref="bool"/>) is passed as the command parameter.
        /// </remarks>
        [Browsable(true)]
        [Category("Data")]
        [Description("The parameter to pass to the command on CheckedChanged event. If not set, Checked is used.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public new object? CommandParameter
        {
            get => _commandParameter;
            set
            {
                _commandParameter = value;
                if (_command != null)
                {
                    Enabled = _command.CanExecute(_commandParameter ?? Checked);
                }
            }
        }

        private void Command_CanExecuteChanged(object? sender, EventArgs e)
            => Enabled = _command?.CanExecute(CommandParameter ?? Checked) ?? true;

        private void CheckBox_CheckedChanged(object? sender, EventArgs e)
        {
            if (_command != null)
            {
                var parameter = CommandParameter ?? Checked;
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
        /// <returns>The current <see cref="LocalizableCheckBoxWithCommandSupport"/> instance to support fluent method chaining.</returns>
        public LocalizableCheckBoxWithCommandSupport BindCommand(ICommand command, object? parameter = null)
        {
            CommandParameter = parameter;
            Command = command;

            return this;
        }
    }
}
