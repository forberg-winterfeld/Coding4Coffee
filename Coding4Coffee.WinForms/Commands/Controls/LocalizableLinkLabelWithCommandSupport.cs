using Coding4Coffee.WinForms.Localization.Controls;
using System.ComponentModel;
using System.Windows.Input;

namespace Coding4Coffee.WinForms.Commands.Controls
{
    /// <summary>
    /// A localizable LinkLabel control that integrates command binding support.
    /// </summary>
    /// <remarks>
    /// Executes the bound command when the link is clicked via the native <see cref="LinkLabel.LinkClicked"/> event.
    /// Inherits from <see cref="LocalizableLinkLabel"/>, meaning dynamic localization applies to
    /// <see cref="LinkLabel.Text"/> at runtime.
    /// Unlike <see cref="Button"/>, LinkLabel is designed for hyperlink-style navigation and actions,
    /// making it suitable for clickable links within forms and dialogs.
    /// </remarks>
    public class LocalizableLinkLabelWithCommandSupport : LocalizableLinkLabel
    {
        private ICommand? _command;
        private object? _commandParameter;

        /// <summary>
        /// Initializes a new instance of the <see cref="LocalizableLinkLabelWithCommandSupport"/> class.
        /// </summary>
        public LocalizableLinkLabelWithCommandSupport()
        {
            LinkClicked += LinkLabel_LinkClicked;
        }

        /// <summary>
        /// Gets or sets the command to execute when the link is clicked.
        /// </summary>
        /// <remarks>
        /// Executed in response to <see cref="LinkLabel.LinkClicked"/>.
        /// </remarks>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public ICommand? Command
        {
            get => _command;
            set
            {
                _command?.CanExecuteChanged -= Command_CanExecuteChanged;

                _command = value;

                if (_command != null)
                {
                    _command.CanExecuteChanged += Command_CanExecuteChanged;
                    Enabled = _command.CanExecute(CommandParameter);
                }
            }
        }

        /// <summary>
        /// Gets or sets an optional command parameter.
        /// </summary>
        /// <remarks>
        /// If this property is <see langword="null"/>, <see langword="null"/> is passed as the command parameter.
        /// </remarks>
        [Browsable(true)]
        [Category("Data")]
        [Description("The parameter to pass to the command on LinkClicked event.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public object? CommandParameter
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

        private void LinkLabel_LinkClicked(object? sender, LinkLabelLinkClickedEventArgs e)
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
        /// <returns>The current <see cref="LocalizableLinkLabelWithCommandSupport"/> instance to support fluent method chaining.</returns>
        public LocalizableLinkLabelWithCommandSupport BindCommand(ICommand command, object? parameter = null)
        {
            CommandParameter = parameter;
            Command = command;

            return this;
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Command = null;
                LinkClicked -= LinkLabel_LinkClicked;
            }
            base.Dispose(disposing);
        }
    }
}
