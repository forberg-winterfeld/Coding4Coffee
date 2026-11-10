using System.Text.Json;

namespace Coding4Coffee.Basics.Configuration
{
    /// <summary>
    /// Represents a typed configuration setting that can optionally be persisted via a <see cref="System.Configuration.SettingsBase"/>.
    /// </summary>
    /// <typeparam name="TValue">The type of the configuration value.</typeparam>
    public class Config<TValue> : IConfig
    {
        private TValue? _value;
        private string? _settingsContent;

        /// <summary>Raised when the value of the configuration changes.</summary>
        public event EventHandler? ValueChanged;

        /// <summary>Initializes a new instance of <see cref="Config{TValue}"/>.</summary>
        public Config() { }

        /// <summary>Initializes a new instance of <see cref="Config{TValue}"/> with a key and optional persistence flag.</summary>
        /// <param name="key">The key under which the config value is stored.</param>
        /// <param name="isTransient">If <c>true</c>, the value is not persisted to the settings file.</param>
        public Config(string key, bool isTransient = false) : this()
        {
            Key = key;
            IsTransient = isTransient;
        }

        /// <summary>The key under which the configuration value is stored.</summary>
        public string Key { get; set; } = string.Empty;

        /// <summary>If <c>true</c>, the configuration value is not persisted to the settings file.</summary>
        public bool IsTransient { get; set; }

        private bool ValueEquals(TValue? other)
        {
            return _value == null ? other == null : _value.Equals(other);
        }

        /// <summary>Indicates whether the configuration has a non-null value.</summary>
        public bool HasValue => Value != null;

        /// <summary>Indicates whether the configuration value equals the default value for <typeparamref name="TValue"/>.</summary>
        public bool HasDefaultValue => HasValue && ValueEquals(default);

        /// <summary>Gets or sets the configuration value.</summary>
        public virtual TValue? Value
        {
            get
            {
                if (!IsTransient && string.IsNullOrWhiteSpace(_settingsContent))
                {
                    try
                    {
                        _settingsContent = ConfigFactory.Settings?[Key]?.ToString();

                        if (!string.IsNullOrWhiteSpace(_settingsContent))
                            _value = JsonSerializer.Deserialize<TValue>(_settingsContent);
                    }
                    catch(JsonException)
                    {
                        // invalid JSON found in Settings file -> clean up
                        _value = default;
                        _settingsContent = null;

                        if(ConfigFactory.Settings != null)
                        {
                            ConfigFactory.Settings[Key] = null;
                            ConfigFactory.Settings.Save();
                        }
                    }
                }

                return _value;
            }

            set
            {
                if (!ValueEquals(value))
                {
                    _value = value;

                    if (!IsTransient)
                    {
                        _settingsContent = JsonSerializer.Serialize(_value);

                        if (ConfigFactory.Settings != null)
                        {
                            ConfigFactory.Settings[Key] = _settingsContent;
                            ConfigFactory.Settings.Save();
                        }
                    }

                    ValueChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>Implicitly converts a <see cref="Config{TValue}"/> instance to its value.</summary>
        /// <param name="config">The config instance.</param>
        public static implicit operator TValue? (Config<TValue> config) => config.Value;
    }
}
