using System.ComponentModel;

namespace Coding4Coffee.WinForms.Configuration.Controls
{
    /// <summary>A track bar that stores its value in an integer configuration setting.</summary>
    /// <remarks>
    /// User and programmatic value changes are stored after <see cref="DebounceInterval"/> milliseconds
    /// without further changes, using the WinForms message loop. Pending changes are saved on unbinding,
    /// rebinding, or disposal unless an external configuration change takes precedence.
    /// A configured value outside the range is displayed at the nearest bound without changing the setting.
    /// Set bounds through this class's properties to reapply the configured value when the range changes.
    /// An unset setting defaults to zero. Configuration notifications from other threads are applied on the
    /// owning UI thread, asynchronously when a handle exists or when it is next created; the configuration
    /// itself is not made thread-safe.
    /// </remarks>
    public class ConfigurationTrackBar : TrackBar, ISupportInitialize
    {
        private readonly ConfigurationValueBinding<int> _binding;
        private string? _configKey;
        private bool _initializing;
        private bool IsDesignEnvironment => DesignMode || LicenseManager.UsageMode == LicenseUsageMode.Designtime;

        /// <summary>Initializes a new configuration-bound track bar.</summary>
        public ConfigurationTrackBar()
        {
            _binding = new(this, () => Value, value => Value = Math.Clamp(value, Minimum, Maximum), true);
            ValueChanged += TrackBar_ValueChanged;
        }

        /// <summary>Gets or sets the delay in milliseconds before a changed value is stored.</summary>
        /// <remarks>Values below 10 milliseconds are clamped to 10 milliseconds.</remarks>
        [Browsable(true)]
        [Category("Behavior")]
        [Description("The delay in milliseconds for the value change debounce.")]
        [DefaultValue(300)]
        public int DebounceInterval
        {
            get => _binding.DebounceInterval;
            set => _binding.DebounceInterval = value;
        }

        /// <summary>Gets or sets the key of the integer configuration setting.</summary>
        /// <remarks>A blank key unbinds without changing the value. The designer does not access configuration.</remarks>
        /// <exception cref="InvalidOperationException">The key is associated with a different configuration type.</exception>
        [Browsable(true)]
        [Category("Configuration")]
        [Description("The key used to store the track bar value in the configuration.")]
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
        public new int Minimum
        {
            get => base.Minimum;
            set => _binding.ChangeRange(() => base.Minimum = value);
        }

        /// <summary>Gets or sets the maximum allowed value.</summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public new int Maximum
        {
            get => base.Maximum;
            set => _binding.ChangeRange(() => base.Maximum = value);
        }

        /// <summary>Sets the minimum and maximum allowed values.</summary>
        /// <param name="minValue">The minimum value.</param>
        /// <param name="maxValue">The maximum value.</param>
        public new void SetRange(int minValue, int maxValue)
            => _binding.ChangeRange(() => base.SetRange(minValue, maxValue));

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

        private void TrackBar_ValueChanged(object? sender, EventArgs e) => _binding.ValueChanged();

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
