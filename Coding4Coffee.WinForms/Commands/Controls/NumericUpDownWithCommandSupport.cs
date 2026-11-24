using System.ComponentModel;
using System.Windows.Input;

namespace Coding4Coffee.WinForms.Commands.Controls
{
    /// <summary>
    /// A custom NumericUpDown control that integrates command binding support.
    /// Executes the bound command when the control's value changes.
    /// </summary>
    public class NumericUpDownWithCommandSupport : NumericUpDown
    {
        private ICommand? _command;
        private object? _commandParameter;

        /// <summary>
        /// Gets or sets the command to execute when the value changes.
        /// </summary>
        /// <remarks>
        /// Executes the command whenever <see cref="NumericUpDown.ValueChanged"/> is raised.
        /// Automatically synchronizes <see cref="Control.Enabled"/> based on <see cref="ICommand.CanExecute(object?)"/>,
        /// passing <see cref="CommandParameter"/> (or <see cref="NumericUpDown.Value"/> if <see cref="CommandParameter"/> is <see langword="null"/>).
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
                    ValueChanged -= NumericUpDown_ValueChanged;
                }

                _command = value;

                if (_command != null)
                {
                    _command.CanExecuteChanged += Command_CanExecuteChanged;
                    ValueChanged += NumericUpDown_ValueChanged;

                    Enabled = _command.CanExecute(CommandParameter ?? Value);
                }   
            }
        }

        /// <summary>
        /// Gets or sets an optional command parameter. If null, Value is passed.
        /// </summary>
        /// <remarks>
        /// If this property is <see langword="null"/>, the control passes its current <see cref="NumericUpDown.Value"/>
        /// (<see cref="decimal"/>) as the command parameter.
        /// </remarks>
        [Browsable(true)]
        [Category("Data")]
        [Description("The parameter to pass to the command on ValueChanged event. If not set, Value is used.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public object? CommandParameter
        {
            get => _commandParameter;
            set
            {
                _commandParameter = value;
                if (_command != null)
                {
                    Enabled = _command.CanExecute(_commandParameter ?? Value);
                }
            }
        }

        private void Command_CanExecuteChanged(object? sender, EventArgs e) 
            => Enabled = _command?.CanExecute(CommandParameter ?? Value) ?? true;

        private void NumericUpDown_ValueChanged(object? sender, EventArgs e)
        {
            if (_command != null)
            {
                var parameter = CommandParameter ?? Value;
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
        /// <returns>The current <see cref="NumericUpDownWithCommandSupport"/> instance to support fluent method chaining.</returns>
        public NumericUpDownWithCommandSupport BindCommand(ICommand command, object? parameter = null)
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