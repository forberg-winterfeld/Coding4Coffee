using Coding4Coffee.WinForms.Localization.Controls;
using System.ComponentModel;
using System.Windows.Input;

namespace Coding4Coffee.WinForms.Commands.Controls
{
    /// <summary>
    /// A localizable TextBox control that integrates command binding support with a debounce delay on text changes.
    /// </summary>
    /// <remarks>
    /// Executes the bound command after text changes stop for the duration of <see cref="DebounceInterval"/>.
    /// Both user input and programmatic text changes restart the timer.
    /// Inherits from <see cref="LocalizableTextBox"/>, meaning dynamic localization applies to
    /// <see cref="TextBox.PlaceholderText"/> rather than <see cref="TextBox.Text"/> to avoid overwriting user input.
    /// </remarks>
    public class LocalizableTextBoxWithCommandSupport : LocalizableTextBox
    {
        private ICommand? _command;
        private object? _commandParameter;
        private readonly System.Windows.Forms.Timer _debounceTimer;

        /// <summary>
        /// Initializes a new instance of the <see cref="LocalizableTextBoxWithCommandSupport"/> class.
        /// </summary>
        public LocalizableTextBoxWithCommandSupport()
        {
            _debounceTimer = new System.Windows.Forms.Timer
            {
                Interval = 300 // Standard: 300 ms Verzögerung
            };
            _debounceTimer.Tick += DebounceTimer_Tick;
        }

        /// <summary>
        /// Gets or sets the delay in milliseconds before the command is executed after typing stops.
        /// </summary>
        /// <remarks>
        /// Any value smaller than 50 milliseconds is clamped to 50 milliseconds via <see cref="Math.Max(int, int)"/>
        /// to maintain responsive UI performance and prevent timer flooding.
        /// </remarks>
        [Browsable(true)]
        [Category("Behavior")]
        [Description("The delay in milliseconds for the text change debounce.")]
        [DefaultValue(300)]
        public int DebounceInterval
        {
            get => _debounceTimer.Interval;
            set => _debounceTimer.Interval = Math.Max(50, value);
        }

        /// <summary>
        /// Gets or sets the command to execute.
        /// </summary>
        /// <remarks>
        /// Text changes are debounced using an internal timer set to <see cref="DebounceInterval"/> ms.
        /// Each text change restarts the timer, and the command executes only after changes cease for that interval.
        /// Automatically synchronizes <see cref="Control.Enabled"/> based on <see cref="ICommand.CanExecute(object?)"/>,
        /// passing <see cref="CommandParameter"/> (or <see cref="TextBox.Text"/> if <see cref="CommandParameter"/> is <see langword="null"/>).
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
                    TextChanged -= TextBox_TextChanged;
                    _debounceTimer.Stop();
                }

                _command = value;

                if (_command != null)
                {
                    _command.CanExecuteChanged += Command_CanExecuteChanged;
                    TextChanged += TextBox_TextChanged;

                    Enabled = _command.CanExecute(CommandParameter ?? Text);
                }
            }
        }

        /// <summary>
        /// Gets or sets an optional command parameter. If null, Text is passed.
        /// </summary>
        /// <remarks>
        /// If this property is <see langword="null"/>, the current <see cref="TextBox.Text"/> string
        /// is passed as the command parameter.
        /// </remarks>
        [Browsable(true)]
        [Category("Data")]
        [Description("The parameter to pass to the command on TextChanged event (debounced). If not set, Text is used.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public object? CommandParameter
        {
            get => _commandParameter;
            set
            {
                _commandParameter = value;
                if (_command != null)
                {
                    Enabled = _command.CanExecute(_commandParameter ?? Text);
                }
            }
        }

        private void Command_CanExecuteChanged(object? sender, EventArgs e)
            => Enabled = _command?.CanExecute(CommandParameter ?? Text) ?? true;

        private void TextBox_TextChanged(object? sender, EventArgs e)
        {
            // Timer bei jedem Tastendruck neu starten (Debounce)
            _debounceTimer.Stop();
            _debounceTimer.Start();
        }

        private void DebounceTimer_Tick(object? sender, EventArgs e)
        {
            _debounceTimer.Stop();

            if (_command != null)
            {
                var parameter = CommandParameter ?? Text;
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
        /// <returns>The current <see cref="LocalizableTextBoxWithCommandSupport"/> instance to support fluent method chaining.</returns>
        public LocalizableTextBoxWithCommandSupport BindCommand(ICommand command, object? parameter = null)
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
                _debounceTimer.Stop();
                _debounceTimer.Tick -= DebounceTimer_Tick;
                _debounceTimer.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}