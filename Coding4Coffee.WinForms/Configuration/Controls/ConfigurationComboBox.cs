using System.ComponentModel;
using Coding4Coffee.Basics.Configuration;
using Coding4Coffee.WinForms.Localization.Controls;

namespace Coding4Coffee.WinForms.Configuration.Controls
{
    /// <summary>
    /// A localizable combo box that stores its selection in a configuration setting.
    /// </summary>
    /// <remarks>
    /// Populate items before assigning <see cref="ConfigKey"/> in Index or Text mode. In Value mode,
    /// configuration is applied once a data source, binding context and value member are available.
    /// The selected index, stable string value, or displayed text is stored according to
    /// <see cref="SelectionStorageMode"/>. The designer does not access application configuration.
    /// User and programmatic selection changes are stored; no selection is stored as <see langword="null"/>.
    /// A configured value without a matching item clears the selection without overwriting the setting.
    /// Configuration notifications from other threads are applied on the owning UI thread, asynchronously
    /// when a handle exists or when it is next created. The configuration itself is not made thread-safe.
    /// Create and access the control on its owning UI thread.
    /// </remarks>
    public class ConfigurationComboBox : LocalizableComboBox, ISupportInitialize
    {
        private readonly int _ownerThreadId = Environment.CurrentManagedThreadId;
        private readonly Lock _updateLock = new();
        private bool _handleReady;
        private bool _updatePending;
        private bool _disposed;
        private bool _updatingSelection;
        private bool _initializing;
        private Config<int?>? _config;
        private Config<string>? _stringConfig;
        private string? _configKey;
        private ConfigurationComboBoxValueMode _selectionStorageMode;
        private object? _activeDataSource;
        private string? _activeValueMember;
        private bool _valueBindingReady;

        private bool IsDesignEnvironment => DesignMode || LicenseManager.UsageMode == LicenseUsageMode.Designtime;

        /// <summary>Initializes a new instance of the <see cref="ConfigurationComboBox"/> class.</summary>
        public ConfigurationComboBox()
        {
            SelectedIndexChanged += ComboBox_SelectedIndexChanged;
        }

        /// <summary>Gets or sets which selection value is stored.</summary>
        /// <remarks>
        /// Index uses <see cref="Config{TValue}"/> of nullable integer and requires stable ordering.
        /// Value uses a string <see cref="ListControl.ValueMember"/> and requires a data source; Text uses
        /// the displayed string. Text may change when localized and duplicate values or text select the
        /// first match. Set this property before <see cref="ConfigKey"/>. Changing between Index and
        /// the string modes while bound requires unbinding and using a key of the appropriate type.
        /// </remarks>
        [Browsable(true)]
        [Category("Configuration")]
        [Description("The selection value stored in the configuration.")]
        [DefaultValue(ConfigurationComboBoxValueMode.Index)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public ConfigurationComboBoxValueMode SelectionStorageMode
        {
            get => _selectionStorageMode;
            set
            {
                if (!Enum.IsDefined(value))
                    throw new InvalidEnumArgumentException(nameof(value), (int)value, typeof(ConfigurationComboBoxValueMode));
                if (_selectionStorageMode == value)
                    return;
                if (!_initializing && !IsDesignEnvironment && !string.IsNullOrWhiteSpace(_configKey) &&
                    (_selectionStorageMode == ConfigurationComboBoxValueMode.Index) != (value == ConfigurationComboBoxValueMode.Index))
                    throw new InvalidOperationException("Unbind ConfigKey before changing between index and string storage modes.");
                if (!_initializing && _stringConfig != null && value == ConfigurationComboBoxValueMode.Value && IsValueSourceReady())
                    ValidateValueMember();

                _selectionStorageMode = value;
                _valueBindingReady = false;
                UpdateSelectionFromConfig();
            }
        }

        /// <summary>Gets or sets the data source used to populate the combo box.</summary>
        /// <remarks>When Value mode is bound, the value member is validated after the data source changes.</remarks>
        [Category("Data")]
        [DefaultValue(null)]
        [RefreshProperties(RefreshProperties.Repaint)]
        [AttributeProvider(typeof(System.ComponentModel.IListSource))]
        public new object? DataSource
        {
            get => base.DataSource;
            set
            {
                base.DataSource = value;
                if (!IsDesignEnvironment && !_initializing && _stringConfig != null && SelectionStorageMode == ConfigurationComboBoxValueMode.Value)
                    UpdateSelectionFromConfig();
            }
        }

        /// <summary>Gets or sets the key of the configuration storing the selection.</summary>
        /// <remarks>
        /// Assigning a different nonblank key loads its value. Null, empty, or whitespace removes the
        /// binding without changing the selection. Set <see cref="SelectionStorageMode"/> first. Value mode
        /// can bind before the data source and value member are set; selection is restored when they are ready.
        /// The designer retains the key without accessing application configuration.
        /// </remarks>
        /// <exception cref="InvalidOperationException">The key is associated with a different configuration type.</exception>
        [Browsable(true)]
        [Category("Configuration")]
        [Description("The key used to store the selection in the configuration.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public string? ConfigKey
        {
            get => _configKey;
            set
            {
                if (_configKey == value)
                    return;
                if (_initializing || IsDesignEnvironment)
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
            if (_config != null || _stringConfig != null)
                throw new InvalidOperationException("Initialization must begin before binding a configuration key.");
            _initializing = true;
        }

        /// <summary>Ends initialization and binds the configured key.</summary>
        public void EndInit()
        {
            if (!_initializing)
                return;
            _initializing = false;
            if (!IsDesignEnvironment)
                BindConfig(_configKey);
        }

        private void BindConfig(string? value)
        {
            Config<int?>? config = null;
            Config<string>? stringConfig = null;
            if (!string.IsNullOrWhiteSpace(value))
            {
                if (SelectionStorageMode == ConfigurationComboBoxValueMode.Value && IsValueSourceReady())
                    ValidateValueMember();
                if (SelectionStorageMode == ConfigurationComboBoxValueMode.Index)
                    config = ConfigFactory.GetConfig<int?>(value);
                else
                    stringConfig = ConfigFactory.GetConfig<string>(value);
            }

            _config?.ValueChanged -= Config_ValueChanged;
            _stringConfig?.ValueChanged -= Config_ValueChanged;
            _config = config;
            _stringConfig = stringConfig;
            _configKey = value;
            _valueBindingReady = false;
            _config?.ValueChanged += Config_ValueChanged;
            _stringConfig?.ValueChanged += Config_ValueChanged;
            UpdateSelectionFromConfig();
        }

        /// <summary>Localizes the items and restores the configured selection after they are refreshed.</summary>
        public override void Localize()
        {
            if (_config == null && _stringConfig == null)
            {
                base.Localize();
                return;
            }

            bool wasUpdating = _updatingSelection;
            _updatingSelection = true;
            try
            {
                base.Localize();
            }
            finally
            {
                _updatingSelection = wasUpdating;
            }

            if (!wasUpdating)
                UpdateSelectionFromConfig();
        }

        /// <inheritdoc/>
        protected override void OnDataSourceChanged(EventArgs e)
        {
            _valueBindingReady = false;
            base.OnDataSourceChanged(e);
            UpdateSelectionFromConfig();
        }

        /// <inheritdoc/>
        protected override void OnValueMemberChanged(EventArgs e)
        {
            _valueBindingReady = false;
            base.OnValueMemberChanged(e);
            UpdateSelectionFromConfig();
        }

        /// <inheritdoc/>
        protected override void OnBindingContextChanged(EventArgs e)
        {
            _valueBindingReady = false;
            base.OnBindingContextChanged(e);
            UpdateSelectionFromConfig();
        }

        /// <inheritdoc/>
        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            lock (_updateLock)
                _handleReady = true;
            ApplyPendingUpdate();
            if (SelectionStorageMode == ConfigurationComboBoxValueMode.Value && !_valueBindingReady)
                UpdateSelectionFromConfig();
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
                _stringConfig?.ValueChanged -= Config_ValueChanged;
                _config = null;
                _stringConfig = null;
            }
            base.Dispose(disposing);
        }

        private void ComboBox_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (_updatingSelection || (SelectionStorageMode == ConfigurationComboBoxValueMode.Value &&
                (!_valueBindingReady || !ReferenceEquals(_activeDataSource, DataSource) || _activeValueMember != ValueMember)))
                return;

            if (_config != null)
                _config.Value = SelectedIndex < 0 ? null : SelectedIndex;
            else
                _stringConfig?.Value = SelectedIndex < 0 ? null : GetSelectionValue(SelectedItem ?? throw new InvalidOperationException("The selected item is null."));
        }

        private void Config_ValueChanged(object? sender, EventArgs e)
        {
            if (Environment.CurrentManagedThreadId == _ownerThreadId)
            {
                UpdateSelectionFromConfig();
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
            UpdateSelectionFromConfig();
        }

        private void UpdateSelectionFromConfig()
        {
            if (_config == null && _stringConfig == null)
                return;

            if (SelectionStorageMode == ConfigurationComboBoxValueMode.Value)
            {
                _valueBindingReady = false;
                if (!IsValueSourceReady())
                    return;
                ValidateValueMember();
            }

            int index = -1;
            if (_config != null)
            {
                int? configuredIndex = _config.Value;
                if (configuredIndex >= 0 && configuredIndex < Items.Count)
                    index = configuredIndex.Value;
            }
            else if (_stringConfig?.Value is string value)
            {
                for (int i = 0; i < Items.Count; i++)
                {
                    var item = Items[i] ?? throw new InvalidOperationException("A combo box item is null.");
                    if (string.Equals(GetSelectionValue(item), value, StringComparison.Ordinal))
                    {
                        index = i;
                        break;
                    }
                }
            }

            bool wasUpdating = _updatingSelection;
            _updatingSelection = true;
            try
            {
                SelectedIndex = index;
            }
            finally
            {
                _updatingSelection = wasUpdating;
            }

            if (SelectionStorageMode == ConfigurationComboBoxValueMode.Value)
            {
                _activeDataSource = DataSource;
                _activeValueMember = ValueMember;
                _valueBindingReady = true;
            }
        }

        private bool IsValueSourceReady() => DataSource != null && DataManager != null && !string.IsNullOrWhiteSpace(ValueMember);

        private string GetSelectionValue(object item)
        {
            if (SelectionStorageMode == ConfigurationComboBoxValueMode.Text)
                return GetItemText(item) ?? string.Empty;

            return GetValueMemberString(item);
        }

        private void ValidateValueMember()
        {
            if (DataSource == null || string.IsNullOrWhiteSpace(ValueMember))
                throw new InvalidOperationException("Value storage requires a data source and a string ValueMember.");
            foreach (var item in Items)
                _ = GetValueMemberString(item);
        }

        private string GetValueMemberString(object item)
        {
            if (DataSource == null || string.IsNullOrWhiteSpace(ValueMember))
                throw new InvalidOperationException("Value storage requires a data source and a string ValueMember.");

            var property = TypeDescriptor.GetProperties(item).Find(ValueMember, true);
            if (property == null || property.PropertyType != typeof(string))
                throw new InvalidOperationException("ValueMember must identify a string property on each item.");

            return property.GetValue(item) as string
                ?? throw new InvalidOperationException("ValueMember values must not be null.");
        }
    }
}
