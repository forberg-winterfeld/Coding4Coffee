using System.ComponentModel;
using Coding4Coffee.WinForms.Localization.Controls;

namespace Coding4Coffee.WinForms.Configuration.Controls
{
    /// <summary>A localizable group box that stores its radio button selection as a string.</summary>
    /// <remarks>
    /// Add radio buttons as direct children and assign each a unique, nonempty string <see cref="Control.Tag"/>.
    /// The tag is stored in a string configuration setting when a button is checked. A null or unmatched
    /// setting clears the selection without changing the setting; adding a matching button restores it.
    /// Duplicate tags select the first match. Unchecking a button alone does not clear the setting; use the
    /// configuration to clear it. Call <see cref="RefreshSelection"/> after changing a button's tag.
    /// Background configuration changes
    /// are applied on the owning UI thread; the configuration itself is not thread-safe.
    /// </remarks>
    public class ConfigurationRadioButtonGroup : LocalizableGroupBox, ISupportInitialize
    {
        private readonly ConfigurationValueBinding<string> _binding;
        private string? _configKey;
        private bool _initializing;
        private bool IsDesignEnvironment => DesignMode || LicenseManager.UsageMode == LicenseUsageMode.Designtime;

        /// <summary>Initializes a configuration-bound radio button group.</summary>
        public ConfigurationRadioButtonGroup()
        {
            _binding = new(this, () => null, ApplySelection, false);
        }

        /// <summary>Gets or sets the key of the string configuration setting.</summary>
        /// <remarks>A blank key unbinds without changing selection. The designer does not access configuration.</remarks>
        /// <exception cref="InvalidOperationException">The key is associated with a different configuration type.</exception>
        [Browsable(true)]
        [Category("Configuration")]
        [Description("The key used to store the selected radio button's tag.")]
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

        /// <summary>Reapplies the configured selection after changing radio button tags.</summary>
        public void RefreshSelection() => _binding.ApplyConfig();

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

        /// <inheritdoc/>
        protected override void OnControlAdded(ControlEventArgs e)
        {
            base.OnControlAdded(e);
            if (e.Control is RadioButton radio)
            {
                radio.CheckedChanged += Radio_CheckedChanged;
                _binding?.ApplyConfig();
            }
        }

        /// <inheritdoc/>
        protected override void OnControlRemoved(ControlEventArgs e)
        {
            if (e.Control is RadioButton radio)
                radio.CheckedChanged -= Radio_CheckedChanged;
            base.OnControlRemoved(e);
            _binding?.ApplyConfig();
        }

        private void ApplySelection(string? key)
        {
            var selected = key == null ? null : Controls.OfType<RadioButton>()
                .FirstOrDefault(radio => radio.Tag is string tag && string.Equals(tag, key, StringComparison.Ordinal));
            foreach (var radio in Controls.OfType<RadioButton>())
                radio.Checked = radio == selected;
        }

        private void Radio_CheckedChanged(object? sender, EventArgs e)
        {
            if (sender is not RadioButton { Checked: true } radio)
                return;
            if (radio.Tag is not string key || string.IsNullOrEmpty(key))
            {
                if (!IsDesignEnvironment && !_initializing && !string.IsNullOrWhiteSpace(_binding.Key))
                    throw new InvalidOperationException("Each selected radio button must have a nonempty string Tag.");
                return;
            }
            _binding.ValueChanged(key);
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    foreach (var radio in Controls.OfType<RadioButton>())
                        radio.CheckedChanged -= Radio_CheckedChanged;
                    _binding.Dispose();
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }
    }
}
