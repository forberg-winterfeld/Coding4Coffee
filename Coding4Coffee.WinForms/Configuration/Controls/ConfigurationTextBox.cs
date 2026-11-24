using Coding4Coffee.Basics.Configuration;
using System.ComponentModel;

namespace Coding4Coffee.WinForms.Configuration.Controls
{
    /// <summary>
    /// A <see cref="TextBox"/> that loads and stores its text through a string configuration value.
    /// </summary>
    /// <remarks>
    /// Assigning a different <see cref="ConfigKey"/> loads that setting into <see cref="TextBox.Text"/>.
    /// Changes to <see cref="TextBox.Text"/> are stored after <see cref="DebounceInterval"/> milliseconds
    /// without further text changes, using the WinForms message loop. Programmatic changes are treated like user input.
    /// An external configuration change takes precedence over any pending edit. Otherwise, pending edits are saved
    /// when the key changes or the control is disposed. Configuration change notifications from other threads are
    /// applied asynchronously on the owning thread, or when the handle is next created. The control must be created
    /// and accessed on its owning UI thread; this does not make the configuration itself thread-safe.
    /// </remarks>
    public class ConfigurationTextBox : TextBox
    {
        private readonly ConfigurationValueBinding<string> _binding;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationTextBox"/> class.
        /// </summary>
        public ConfigurationTextBox()
        {
            _binding = new(this, () => Text, value => Text = value ?? string.Empty, true);
            TextChanged += ConfigurationTextBox_TextChanged;
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

        /// <summary>
        /// Gets or sets the key of the string configuration value bound to this text box.
        /// </summary>
        /// <remarks>
        /// When a different key that is not null, empty, or whitespace is assigned, the control retrieves
        /// the corresponding setting via <see cref="ConfigFactory.GetConfig{T}(string, bool)"/> and loads
        /// its value into <see cref="TextBox.Text"/>. If the setting has no value, the text is cleared.
        /// Setting this property to <see langword="null"/>, an empty string, or whitespace removes the
        /// configuration binding without changing the text.
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        /// The key is already associated with a configuration of a different type.
        /// </exception>
        [Browsable(true)]
        [Category("Configuration")]
        [Description("The key used to retrieve the configuration's text value from the settings.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public string? ConfigKey
        {
            get => _binding.Key;
            set => _binding.Key = value;
        }

        private void ConfigurationTextBox_TextChanged(object? sender, EventArgs e) => _binding.ValueChanged();

        /// <summary>
        /// Releases the control's resources and unsubscribes from configuration changes.
        /// </summary>
        /// <param name="disposing">Whether managed resources should be released.</param>
        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    TextChanged -= ConfigurationTextBox_TextChanged;
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
