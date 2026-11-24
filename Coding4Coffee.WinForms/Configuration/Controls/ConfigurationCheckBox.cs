using System.ComponentModel;
using Coding4Coffee.Basics.Configuration;
using Coding4Coffee.WinForms.Localization.Controls;

namespace Coding4Coffee.WinForms.Configuration.Controls
{
    /// <summary>
    /// A localizable check box that stores its check state in a configuration setting.
    /// </summary>
    /// <remarks>
    /// By default the control stores all three states in a <see cref="Config{TValue}"/> of <see cref="CheckState"/>.
    /// <see cref="ValueMode"/> can instead select Boolean storage. In Boolean mode, an indeterminate
    /// state is treated as checked and normalized to <see cref="CheckState.Checked"/>; use
    /// <see cref="CheckBox.ThreeState"/> only with CheckState storage when all three states must be preserved.
    /// User and programmatic changes are stored, and configuration changes update the control.
    /// Notifications from other threads are applied on the owning UI thread, asynchronously when a handle
    /// exists or when it is next created. Create and access the control on its owning UI thread; this does
    /// not make the configuration itself thread-safe.
    /// </remarks>
    public class ConfigurationCheckBox : LocalizableCheckBox, ISupportInitialize
    {
        private readonly int _ownerThreadId = Environment.CurrentManagedThreadId;
        private readonly Lock _updateLock = new();
        private bool _handleReady;
        private bool _updatePending;
        private bool _disposed;
        private bool _updatingState;
        private bool _initializing;
        private Config<CheckState>? _config;
        private Config<bool>? _booleanConfig;
        private string? _configKey;
        private ConfigurationCheckBoxValueMode _valueMode;

        /// <summary>Initializes a new instance of the <see cref="ConfigurationCheckBox"/> class.</summary>
        public ConfigurationCheckBox()
        {
            CheckStateChanged += CheckBox_CheckStateChanged;
        }

        /// <summary>Gets or sets the type used to store the check box state.</summary>
        /// <remarks>
        /// The default is <see cref="ConfigurationCheckBoxValueMode.CheckState"/>. Select
        /// <see cref="ConfigurationCheckBoxValueMode.Boolean"/> to use <see cref="Config{TValue}"/> of
        /// <see cref="bool"/>. Unbind <see cref="ConfigKey"/> before changing modes; the new mode requires
        /// a key associated with the corresponding type.
        /// </remarks>
        [Browsable(true)]
        [Category("Configuration")]
        [Description("The type used to store the check box state.")]
        [DefaultValue(ConfigurationCheckBoxValueMode.CheckState)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public ConfigurationCheckBoxValueMode ValueMode
        {
            get => _valueMode;
            set
            {
                if (!Enum.IsDefined(value))
                    throw new InvalidEnumArgumentException(nameof(value), (int)value, typeof(ConfigurationCheckBoxValueMode));
                if (_valueMode == value)
                    return;
                if (!_initializing && !string.IsNullOrWhiteSpace(_configKey))
                    throw new InvalidOperationException("Unbind ConfigKey before changing the configuration value mode.");
                _valueMode = value;
            }
        }

        /// <summary>Gets or sets the key of the configuration storing the check state.</summary>
        /// <remarks>
        /// A nonblank key loads its value according to <see cref="ValueMode"/>. Null, empty, or whitespace
        /// removes the binding without changing the state. An unset setting defaults to unchecked or false.
        /// Set <see cref="ValueMode"/> before the key, unless using designer initialization.
        /// </remarks>
        /// <exception cref="InvalidOperationException">The key is already associated with a different configuration type.</exception>
        [Browsable(true)]
        [Category("Configuration")]
        [Description("The key used to store the check state in the configuration.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public string? ConfigKey
        {
            get => _configKey;
            set
            {
                if (_configKey == value)
                    return;
                if (_initializing)
                {
                    _configKey = value;
                    return;
                }
                BindConfig(value);
            }
        }

        /// <summary>Begins a batch of designer property assignments.</summary>
        public void BeginInit()
        {
            if (_config != null || _booleanConfig != null)
                throw new InvalidOperationException("Initialization must begin before binding a configuration key.");
            _initializing = true;
        }

        /// <summary>Ends initialization and binds the configured key.</summary>
        public void EndInit()
        {
            if (!_initializing)
                return;
            BindConfig(_configKey);
            _initializing = false;
        }

        /// <inheritdoc/>
        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            lock (_updateLock)
                _handleReady = true;
            ApplyPendingUpdate();
        }

        /// <inheritdoc/>
        protected override void OnHandleDestroyed(EventArgs e)
        {
            lock (_updateLock)
            {
                _handleReady = false;
                _updatePending = true;
            }
            base.OnHandleDestroyed(e);
        }

        /// <summary>Unsubscribes from configuration changes and releases the control's resources.</summary>
        /// <param name="disposing">Whether managed resources should be released.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                lock (_updateLock)
                    _disposed = true;
                _config?.ValueChanged -= Config_ValueChanged;
                _booleanConfig?.ValueChanged -= Config_ValueChanged;
                _config = null;
                _booleanConfig = null;
            }
            base.Dispose(disposing);
        }

        private void CheckBox_CheckStateChanged(object? sender, EventArgs e)
        {
            if (_updatingState)
                return;
            if (_config != null)
                _config.Value = CheckState;
            else if (_booleanConfig != null)
            {
                _booleanConfig.Value = Checked;
                if (CheckState == CheckState.Indeterminate)
                    UpdateCheckStateFromConfig();
            }
        }

        private void Config_ValueChanged(object? sender, EventArgs e)
        {
            if (Environment.CurrentManagedThreadId == _ownerThreadId)
            {
                UpdateCheckStateFromConfig();
                return;
            }

            lock (_updateLock)
            {
                if (_disposed)
                    return;
                _updatePending = true;
                if (_handleReady)
                {
                    try
                    {
                        BeginInvoke(ApplyPendingUpdate);
                    }
                    catch (InvalidOperationException) when (!IsHandleCreated)
                    {
                        // The pending update will be applied when the handle is recreated.
                    }
                }
            }
        }

        private void ApplyPendingUpdate()
        {
            lock (_updateLock)
            {
                if (_disposed || !_updatePending)
                    return;
                _updatePending = false;
            }
            UpdateCheckStateFromConfig();
        }

        private void BindConfig(string? key)
        {
            Config<CheckState>? config = null;
            Config<bool>? booleanConfig = null;
            if (!string.IsNullOrWhiteSpace(key))
            {
                if (ValueMode == ConfigurationCheckBoxValueMode.Boolean)
                    booleanConfig = ConfigFactory.GetConfig<bool>(key);
                else
                    config = ConfigFactory.GetConfig<CheckState>(key);
            }

            _config?.ValueChanged -= Config_ValueChanged;
            _booleanConfig?.ValueChanged -= Config_ValueChanged;
            _config = config;
            _booleanConfig = booleanConfig;
            _configKey = key;
            _config?.ValueChanged += Config_ValueChanged;
            _booleanConfig?.ValueChanged += Config_ValueChanged;
            UpdateCheckStateFromConfig();
        }

        private void UpdateCheckStateFromConfig()
        {
            if (_config == null && _booleanConfig == null)
                return;

            _updatingState = true;
            try
            {
                if (_config != null)
                    CheckState = _config.Value;
                else if (_booleanConfig != null)
                    CheckState = _booleanConfig.Value ? CheckState.Checked : CheckState.Unchecked;
            }
            finally
            {
                _updatingState = false;
            }
        }
    }
}
