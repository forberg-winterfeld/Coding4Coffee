using System.ComponentModel;

namespace Coding4Coffee.WinForms.Configuration.Controls
{
    /// <summary>A date/time picker bound to a nullable date/time configuration setting.</summary>
    /// <remarks>
    /// The setting is a <see cref="Coding4Coffee.Basics.Configuration.Config{TValue}"/> of nullable
    /// <see cref="DateTime"/>. Set <see cref="DateTimePicker.ShowCheckBox"/> to allow a null value to be
    /// selected. A null value is displayed unchecked; without the checkbox, the current picker value remains
    /// visible and the setting stays null until the value changes. Out-of-range values are displayed at the
    /// nearest bound without changing the setting. Set bounds through this class to reapply the stored value.
    /// User and programmatic changes are stored immediately. Background notifications are applied on the
    /// owning UI thread; the configuration itself is not thread-safe.
    /// </remarks>
    public class ConfigurationDateTimePicker : DateTimePicker, ISupportInitialize
    {
        private readonly ConfigurationValueBinding<DateTime?> _binding;
        private string? _configKey;
        private bool _initializing;
        private bool IsDesignEnvironment => DesignMode || LicenseManager.UsageMode == LicenseUsageMode.Designtime;

        /// <summary>Initializes a configuration-bound date/time picker.</summary>
        public ConfigurationDateTimePicker()
        {
            _binding = new(this, () => ShowCheckBox && !Checked ? null : Value, ApplyDate, false);
            ValueChanged += Picker_ValueChanged;
        }

        /// <summary>Gets or sets the key of the nullable date/time configuration setting.</summary>
        /// <remarks>A blank key unbinds without changing the picker. The designer does not access configuration.</remarks>
        /// <exception cref="InvalidOperationException">The key is associated with a different configuration type.</exception>
        [Browsable(true)]
        [Category("Configuration")]
        [Description("The key used to store the nullable date/time in the configuration.")]
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

        /// <summary>Gets or sets the minimum allowed date.</summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public new DateTime MinDate
        {
            get => base.MinDate;
            set => _binding.ChangeRange(() => base.MinDate = value);
        }

        /// <summary>Gets or sets the maximum allowed date.</summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public new DateTime MaxDate
        {
            get => base.MaxDate;
            set => _binding.ChangeRange(() => base.MaxDate = value);
        }

        /// <summary>Gets or sets whether the picker displays a checkbox for an optional value.</summary>
        [DefaultValue(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public new bool ShowCheckBox
        {
            get => base.ShowCheckBox;
            set => _binding.ChangeRange(() => base.ShowCheckBox = value);
        }

        /// <summary>Gets or sets whether the optional date is selected.</summary>
        /// <remarks>Set this property on the configuration picker to synchronize programmatic changes.</remarks>
        [DefaultValue(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public new bool Checked
        {
            get => base.Checked;
            set
            {
                if (base.Checked == value)
                    return;
                base.Checked = value;
                if (ShowCheckBox)
                    _binding.ValueChanged();
            }
        }

        /// <summary>Begins a batch of designer property assignments.</summary>
        public void BeginInit()
        {
            if (!string.IsNullOrWhiteSpace(_binding.Key))
                throw new InvalidOperationException("Initialization must begin before binding a configuration key.");
            _initializing = true;
        }

        /// <summary>Ends initialization and binds the configured key.</summary>
        public void EndInit()
        {
            if (!_initializing)
                return;
            if (!IsDesignEnvironment)
                _binding.Key = _configKey;
            _initializing = false;
        }

        private void ApplyDate(DateTime? date)
        {
            if (date.HasValue)
            {
                Value = date.Value < MinDate ? MinDate : date.Value > MaxDate ? MaxDate : date.Value;
                if (ShowCheckBox)
                    Checked = true;
            }
            else if (ShowCheckBox)
                Checked = false;
        }

        private void Picker_ValueChanged(object? sender, EventArgs e) => _binding.ValueChanged();

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
