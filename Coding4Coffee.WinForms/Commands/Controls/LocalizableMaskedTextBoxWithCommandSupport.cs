using Coding4Coffee.WinForms.Localization.Controls;
using System.ComponentModel;
using System.Windows.Input;

namespace Coding4Coffee.WinForms.Commands.Controls
{
    /// <summary>
    /// A localizable MaskedTextBox control that integrates command binding support with debounce delay on text changes
    /// and optional error handling on mask input rejection.
    /// </summary>
    /// <remarks>
    /// Provides two independent commands for different scenarios:
    /// 1. <see cref="TextCommand"/>: Executes after the user pauses typing for <see cref="DebounceInterval"/> milliseconds.
    ///    Useful for live validation or filtering based on formatted input.
    /// 2. <see cref="MaskRejectionCommand"/>: Executes when the user attempts to enter characters that violate the mask.
    ///    Useful for error feedback or logging of invalid input attempts.
    /// Inherits from <see cref="LocalizableMaskedTextBox"/>, meaning dynamic localization applies to
    /// <c>Mask</c> and <c>Text</c> at runtime.
    /// </remarks>
    public class LocalizableMaskedTextBoxWithCommandSupport : LocalizableMaskedTextBox
    {
        private ICommand? _textCommand;
        private object? _textCommandParameter;
        private readonly System.Windows.Forms.Timer _debounceTimer;

        private ICommand? _maskRejectionCommand;
        private object? _maskRejectionCommandParameter;

        /// <summary>
        /// Initializes a new instance of the <see cref="LocalizableMaskedTextBoxWithCommandSupport"/> class.
        /// </summary>
        public LocalizableMaskedTextBoxWithCommandSupport()
        {
            _debounceTimer = new System.Windows.Forms.Timer
            {
                Interval = 300 // Standard: 300 ms delay
            };
            _debounceTimer.Tick += DebounceTimer_Tick;

            TextChanged += MaskedTextBox_TextChanged;
            MaskInputRejected += MaskedTextBox_MaskInputRejected;
        }

        /// <summary>
        /// Gets or sets the delay in milliseconds before the text command is executed after typing stops.
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

        #region Text Command
        /// <summary>
        /// Gets or sets the command to execute when valid text changes after the debounce interval.
        /// </summary>
        /// <remarks>
        /// Executed in response to <see cref="Control.TextChanged"/> event after <see cref="DebounceInterval"/> milliseconds of inactivity.
        /// This command fires only for text changes that conform to the mask (rejected characters do not trigger this command).
        /// Automatically synchronizes <see cref="Control.Enabled"/> based on <see cref="ICommand.CanExecute(object?)"/>,
        /// passing <see cref="TextCommandParameter"/> (or <see cref="TextBoxBase.Text"/> if <see cref="TextCommandParameter"/> is <see langword="null"/>).
        /// </remarks>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ICommand? TextCommand
        {
            get => _textCommand;
            set
            {
                if (_textCommand != null)
                {
                    _textCommand.CanExecuteChanged -= TextCommand_CanExecuteChanged;
                    _debounceTimer.Stop();
                }

                _textCommand = value;

                if (_textCommand != null)
                {
                    _textCommand.CanExecuteChanged += TextCommand_CanExecuteChanged;
                    Enabled = _textCommand.CanExecute(TextCommandParameter ?? Text);
                }
            }
        }

        /// <summary>
        /// Gets or sets an optional command parameter for <see cref="TextCommand"/>. If null, Text is passed.
        /// </summary>
        /// <remarks>
        /// If this property is <see langword="null"/>, the current <see cref="TextBoxBase.Text"/> string
        /// is passed as the command parameter on TextChanged event (debounced).
        /// </remarks>
        [Browsable(true)]
        [Category("Data")]
        [Description("The parameter to pass to the TextCommand on TextChanged event (debounced). If not set, Text is used.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public object? TextCommandParameter
        {
            get => _textCommandParameter;
            set
            {
                _textCommandParameter = value;
                if (_textCommand != null)
                {
                    Enabled = _textCommand.CanExecute(_textCommandParameter ?? Text);
                }
            }
        }

        private void TextCommand_CanExecuteChanged(object? sender, EventArgs e)
            => Enabled = _textCommand?.CanExecute(TextCommandParameter ?? Text) ?? true;

        private void MaskedTextBox_TextChanged(object? sender, EventArgs e)
        {
            // Restart timer on each keystroke (Debounce)
            _debounceTimer.Stop();
            _debounceTimer.Start();
        }

        private void DebounceTimer_Tick(object? sender, EventArgs e)
        {
            _debounceTimer.Stop();

            if (_textCommand != null)
            {
                var parameter = TextCommandParameter ?? Text;
                if (_textCommand.CanExecute(parameter))
                {
                    _textCommand.Execute(parameter);
                }
            }
        }

        /// <summary>
        /// Binds a text command and optional parameter to the control.
        /// </summary>
        /// <param name="command">The command to assign.</param>
        /// <param name="parameter">The parameter value for the command.</param>
        /// <returns>The current <see cref="LocalizableMaskedTextBoxWithCommandSupport"/> instance to support fluent method chaining.</returns>
        public LocalizableMaskedTextBoxWithCommandSupport BindTextCommand(ICommand command, object? parameter = null)
        {
            TextCommandParameter = parameter;
            TextCommand = command;

            return this;
        }
        #endregion

        #region Mask Rejection Command
        /// <summary>
        /// Gets or sets the command to execute when the user attempts to enter characters that violate the mask.
        /// </summary>
        /// <remarks>
        /// Executed in response to <see cref="MaskedTextBox.MaskInputRejected"/> event.
        /// Useful for logging, error feedback, or sound/visual alerts when invalid input is attempted.
        /// The command parameter defaults to <see langword="null"/> or can be set to a custom value.
        /// </remarks>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ICommand? MaskRejectionCommand
        {
            get => _maskRejectionCommand;
            set => _maskRejectionCommand = value;
        }

        /// <summary>
        /// Gets or sets an optional command parameter for <see cref="MaskRejectionCommand"/>.
        /// </summary>
        /// <remarks>
        /// This parameter is passed to the command whenever mask input rejection occurs.
        /// If <see langword="null"/>, the command still executes but with a null parameter.
        /// </remarks>
        [Browsable(true)]
        [Category("Data")]
        [Description("The parameter to pass to the MaskRejectionCommand on MaskInputRejected event.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public object? MaskRejectionCommandParameter
        {
            get => _maskRejectionCommandParameter;
            set => _maskRejectionCommandParameter = value;
        }

        private void MaskedTextBox_MaskInputRejected(object? sender, MaskInputRejectedEventArgs e)
        {
            if (_maskRejectionCommand != null && _maskRejectionCommand.CanExecute(MaskRejectionCommandParameter))
            {
                _maskRejectionCommand.Execute(MaskRejectionCommandParameter);
            }
        }

        /// <summary>
        /// Binds a mask rejection command and optional parameter to the control.
        /// </summary>
        /// <param name="command">The command to assign.</param>
        /// <param name="parameter">The parameter value for the command.</param>
        /// <returns>The current <see cref="LocalizableMaskedTextBoxWithCommandSupport"/> instance to support fluent method chaining.</returns>
        public LocalizableMaskedTextBoxWithCommandSupport BindMaskRejectionCommand(ICommand command, object? parameter = null)
        {
            MaskRejectionCommandParameter = parameter;
            MaskRejectionCommand = command;

            return this;
        }
        #endregion

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
