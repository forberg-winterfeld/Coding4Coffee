using System.ComponentModel;
using System.Windows.Input;

namespace Coding4Coffee.WinForms.Commands.Controls
{
    /// <summary>
    /// A custom TrackBar control that integrates command binding support with an optional debounce delay.
    /// Executes the bound command when the trackbar value changes.
    /// </summary>
    /// <remarks>
    /// Debounces command execution during slider dragging using an internal timer set to <see cref="DebounceInterval"/> ms.
    /// This prevents flooding the command with updates while the user drags the slider thumb.
    /// </remarks>
    public class TrackBarWithCommandSupport : TrackBar
    {
        private ICommand? _command;
        private object? _commandParameter;
        private readonly System.Windows.Forms.Timer _debounceTimer;

        /// <summary>
        /// Initializes a new instance of the <see cref="TrackBarWithCommandSupport"/> class.
        /// </summary>
        public TrackBarWithCommandSupport()
        {
            _debounceTimer = new System.Windows.Forms.Timer
            {
                Interval = 300 // Standard: 300 ms Verzögerung beim Ziehen
            };
            _debounceTimer.Tick += DebounceTimer_Tick;
        }

        /// <summary>
        /// Gets or sets the delay in milliseconds before the command is executed after sliding stops.
        /// </summary>
        /// <remarks>
        /// Any value smaller than 10 milliseconds is clamped to 10 milliseconds via <see cref="Math.Max(int, int)"/>
        /// to prevent timer flooding during rapid trackbar scrolling.
        /// </remarks>
        [Browsable(true)]
        [Category("Behavior")]
        [Description("The delay in milliseconds for the value change debounce.")]
        [DefaultValue(300)]
        public int DebounceInterval
        {
            get => _debounceTimer.Interval;
            set => _debounceTimer.Interval = Math.Max(10, value);
        }

        /// <summary>
        /// Gets or sets the command to execute when the value changes.
        /// </summary>
        /// <remarks>
        /// Listens to <see cref="TrackBar.Scroll"/> and defers execution until scrolling pauses for
        /// <see cref="DebounceInterval"/> ms.
        /// Automatically synchronizes <see cref="Control.Enabled"/> based on <see cref="ICommand.CanExecute(object?)"/>,
        /// passing <see cref="CommandParameter"/> (or <see cref="TrackBar.Value"/> if <see cref="CommandParameter"/> is <see langword="null"/>).
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
                    Scroll -= TrackBar_Scroll;
                    _debounceTimer.Stop();
                }

                _command = value;

                if (_command != null)
                {
                    _command.CanExecuteChanged += Command_CanExecuteChanged;
                    Scroll += TrackBar_Scroll;

                    Enabled = _command.CanExecute(CommandParameter ?? Value);
                }   
            }
        }

        /// <summary>
        /// Gets or sets an optional command parameter. If null, Value is passed.
        /// </summary>
        /// <remarks>
        /// If this property is <see langword="null"/>, the control passes its current <see cref="TrackBar.Value"/>
        /// (<see cref="int"/>) as the command parameter.
        /// </remarks>
        [Browsable(true)]
        [Category("Data")]
        [Description("The parameter to pass to the command on Scroll event (debounced). If not set, Value is used.")]
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

        private void TrackBar_Scroll(object? sender, EventArgs e)
        {
            // Timer bei jeder Bewegung neu starten, um Feuermarkt-Fluten zu verhindern
            _debounceTimer.Stop();
            _debounceTimer.Start();
        }

        private void DebounceTimer_Tick(object? sender, EventArgs e)
        {
            _debounceTimer.Stop();

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
        /// <returns>The current <see cref="TrackBarWithCommandSupport"/> instance to support fluent method chaining.</returns>
        public TrackBarWithCommandSupport BindCommand(ICommand command, object? parameter = null)
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
                _debounceTimer?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}