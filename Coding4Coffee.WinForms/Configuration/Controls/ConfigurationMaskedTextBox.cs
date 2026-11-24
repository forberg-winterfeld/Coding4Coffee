using System.ComponentModel;

namespace Coding4Coffee.WinForms.Configuration.Controls
{
    /// <summary>A masked text box that stores its text in a string configuration setting.</summary>
    /// <remarks>
    /// Set <see cref="MaskedTextBox.Mask"/> and <see cref="MaskedTextBox.TextMaskFormat"/> before binding
    /// <see cref="ConfigKey"/>. WinForms applies the mask when loading text and the selected text format
    /// determines which literals and prompt characters are stored. Values that the mask cannot represent
    /// are not written back until the text changes. User and programmatic changes are stored after
    /// <see cref="DebounceInterval"/> milliseconds of inactivity on the WinForms message loop. External
    /// configuration updates take precedence over pending text; otherwise pending text is saved on unbinding,
    /// rebinding, or disposal. Background notifications are applied on the owning UI thread; the configuration
    /// itself is not thread-safe. Create and access the control on its owning UI thread.
    /// </remarks>
    public class ConfigurationMaskedTextBox : MaskedTextBox, ISupportInitialize
    {
        private readonly ConfigurationValueBinding<string> _binding;
        private string? _configKey;
        private bool _initializing;
        private bool IsDesignEnvironment => DesignMode || LicenseManager.UsageMode == LicenseUsageMode.Designtime;

        /// <summary>Initializes a configuration-bound masked text box.</summary>
        public ConfigurationMaskedTextBox()
        {
            _binding = new(this, () => Text, value => Text = value ?? string.Empty, true);
            TextChanged += TextBox_TextChanged;
        }

        /// <summary>Gets or sets the delay in milliseconds before changed text is stored.</summary>
        /// <remarks>Values below 50 milliseconds are clamped to 50 milliseconds.</remarks>
        [Browsable(true)]
        [Category("Behavior")]
        [Description("The delay in milliseconds for the text change debounce.")]
        [DefaultValue(300)]
        public int DebounceInterval
        {
            get => _binding.DebounceInterval;
            set => _binding.DebounceInterval = Math.Max(50, value);
        }

        /// <summary>Gets or sets the key of the string configuration setting.</summary>
        /// <remarks>A blank key removes the binding without changing the displayed text. The designer does not access configuration.</remarks>
        /// <exception cref="InvalidOperationException">The key is associated with a different configuration type.</exception>
        [Browsable(true)]
        [Category("Configuration")]
        [Description("The key used to store the text in the configuration.")]
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

        private void TextBox_TextChanged(object? sender, EventArgs e) => _binding.ValueChanged();

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    TextChanged -= TextBox_TextChanged;
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
