using System.ComponentModel;

namespace Coding4Coffee.WinForms.Configuration.Controls
{
    /// <summary>A numeric input that stores its value in a decimal configuration setting.</summary>
    /// <remarks>
    /// User and programmatic value changes are stored immediately. A configured value outside the control's
    /// range is displayed at the nearest bound without changing the setting. Set bounds through this class's
    /// properties to reapply the configured value when the range changes. An unset setting defaults to zero.
    /// Configuration notifications from other threads are applied on the owning UI thread, asynchronously when
    /// a handle exists or when it is next created. This does not make the configuration itself thread-safe.
    /// </remarks>
    public class ConfigurationNumericUpDown : NumericUpDown, ISupportInitialize
    {
        private readonly ConfigurationValueBinding<decimal> _binding;
        private string? _configKey;
        private bool _initializing;
        private bool IsDesignEnvironment => DesignMode || LicenseManager.UsageMode == LicenseUsageMode.Designtime;

        /// <summary>Initializes a new configuration-bound numeric input.</summary>
        public ConfigurationNumericUpDown()
        {
            _binding = new(this, () => Value, value => Value = Math.Clamp(value, Minimum, Maximum), false);
            ValueChanged += NumericUpDown_ValueChanged;
        }

        /// <summary>Gets or sets the key of the decimal configuration setting.</summary>
        /// <remarks>A blank key unbinds without changing the value. The designer does not access configuration.</remarks>
        /// <exception cref="InvalidOperationException">The key is associated with a different configuration type.</exception>
        [Browsable(true)]
        [Category("Configuration")]
        [Description("The key used to store the numeric value in the configuration.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public string? ConfigKey
        {
            get => _configKey;
            set
            {
                if (_configKey == value)
                    return;
                if (!_initializing && !IsDesignEnvironment)
                    _binding.Key = value;
                _configKey = value;
            }
        }

        /// <summary>Gets or sets the minimum allowed value.</summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public new decimal Minimum
        {
            get => base.Minimum;
            set => _binding.ChangeRange(() => base.Minimum = value);
        }

        /// <summary>Gets or sets the maximum allowed value.</summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public new decimal Maximum
        {
            get => base.Maximum;
            set => _binding.ChangeRange(() => base.Maximum = value);
        }

        /// <summary>Begins a batch of designer property assignments.</summary>
        public new void BeginInit()
        {
            if (!string.IsNullOrWhiteSpace(_binding.Key))
                throw new InvalidOperationException("Initialization must begin before binding a configuration key.");
            _initializing = true;
            base.BeginInit();
        }

        /// <summary>Ends initialization and binds the configured key.</summary>
        public new void EndInit()
        {
            base.EndInit();
            if (!_initializing)
                return;
            if (!IsDesignEnvironment)
                _binding.Key = _configKey;
            _initializing = false;
        }

        private void NumericUpDown_ValueChanged(object? sender, EventArgs e) => _binding.ValueChanged();

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                    _binding.Dispose();
            }
            finally
            {
                base.Dispose(disposing);
            }
        }
    }
}
