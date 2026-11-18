using Coding4Coffee.WinForms.Localization.Controls;
using System.ComponentModel;
using System.Windows.Input;

namespace Coding4Coffee.WinForms.Commands.Controls
{
    /// <summary>
    /// A localizable ToolStripSplitButton control that integrates command binding support.
    /// Executes the bound command when the button portion is clicked.
    /// </summary>
    public class LocalizableToolStripSplitButtonWithCommandSupport : LocalizableToolStripSplitButton
    {
        private ICommand? _command;
        private object? _commandParameter;

        /// <summary>
        /// Gets or sets the command to execute when the button portion is clicked.
        /// </summary>
        /// <remarks>
        /// Shadows the inherited <see cref="ToolStripItem.Command"/> property intentionally.
        /// Executes the command whenever the button is clicked (not the dropdown arrow).
        /// Uses <see cref="ToolStripSplitButton.ButtonClick"/> instead of the default Click event.
        /// Automatically synchronizes <see cref="ToolStripItem.Enabled"/> based on <see cref="ICommand.CanExecute(object?)"/>,
        /// passing <see cref="CommandParameter"/> (or <see langword="null"/> if <see cref="CommandParameter"/> is <see langword="null"/>).
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
                    ButtonClick -= ToolStripSplitButton_ButtonClick;
                }

                _command = value;

                if (_command != null)
                {
                    _command.CanExecuteChanged += Command_CanExecuteChanged;
                    ButtonClick += ToolStripSplitButton_ButtonClick;

                    Enabled = _command.CanExecute(CommandParameter);
                }
            }
        }

        /// <summary>
        /// Gets or sets an optional command parameter.
        /// </summary>
        /// <remarks>
        /// Shadows the inherited <see cref="ToolStripItem.CommandParameter"/> property intentionally.
        /// If this property is <see langword="null"/>, <see langword="null"/> is passed as the command parameter.
        /// </remarks>
        [Browsable(true)]
        [Category("Data")]
        [Description("The parameter to pass to the command on ButtonClick event.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public new object? CommandParameter
        {
            get => _commandParameter;
            set
            {
                _commandParameter = value;
                if (_command != null)
                {
                    Enabled = _command.CanExecute(_commandParameter);
                }
            }
        }

        private void Command_CanExecuteChanged(object? sender, EventArgs e)
            => Enabled = _command?.CanExecute(CommandParameter) ?? true;

        private void ToolStripSplitButton_ButtonClick(object? sender, EventArgs e)
        {
            if (_command != null && _command.CanExecute(CommandParameter))
            {
                _command.Execute(CommandParameter);
            }
        }

        /// <summary>
        /// Binds a command and optional parameter to the control.
        /// </summary>
        /// <param name="command">The command to assign.</param>
        /// <param name="parameter">The parameter value for the command.</param>
        /// <returns>The current <see cref="LocalizableToolStripSplitButtonWithCommandSupport"/> instance to support fluent method chaining.</returns>
        public LocalizableToolStripSplitButtonWithCommandSupport BindCommand(ICommand command, object? parameter = null)
        {
            CommandParameter = parameter;
            Command = command;

            return this;
        }
    }
}
