using System.Configuration;

namespace Coding4Coffee.Basics.Configuration
{
    /// <summary>
    /// Factory class for creating and managing typed configuration settings.
    /// </summary>
    public static class ConfigFactory
    {
        /// <summary>The settings object used to persist configuration values.</summary>
        public static SettingsBase? Settings { get; set; }

        private static readonly Dictionary<string, IConfig> _configs = [];

        /// <summary>
        /// Creates or retrieves a standard configuration setting of value type <typeparamref name="T"/> by its key.
        /// </summary>
        /// <typeparam name="T">The value type of the configuration setting.</typeparam>
        /// <param name="key">The key under which the config value is stored.</param>
        /// <param name="isTransient">Controls whether the setting is persisted to a settings file.</param>
        /// <returns>A <see cref="Config{T}"/> instance associated with the specified key.</returns>
        public static Config<T> GetConfig<T>(string key, bool isTransient = false) 
            => GetCustomConfig<Config<T>>(key, isTransient);

        /// <summary>
        /// Creates or retrieves a specialized configuration setting by its key.
        /// </summary>
        /// <typeparam name="TConfig">The type of configuration to retrieve.</typeparam>
        /// <param name="key">The key under which the config value is stored.</param>
        /// <param name="isTransient">If <c>true</c>, the value is not persisted to the settings file.</param>
        /// <returns>The <typeparamref name="TConfig"/> instance associated with the specified key.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the key is already associated with a different config type.</exception>
        public static TConfig GetCustomConfig<TConfig>(string key, bool isTransient = false)
            where TConfig : IConfig, new()
        {
            if (!_configs.TryGetValue(key, out IConfig? config))
            {
                config = new TConfig()
                {
                    Key = key,
                    IsTransient = isTransient
                };
                _configs.Add(key, config);
            }

            if (config is TConfig typedConfig)
                return typedConfig;
            else
                throw new InvalidOperationException("That key has already been used with another config type.");
        }

        /// <summary>Gets or creates a <see cref="CultureInfoConfig"/>.</summary>
        public static CultureInfoConfig GetCultureInfoConfig(string key, bool isTransient = false) => GetCustomConfig<CultureInfoConfig>(key, isTransient);
        /// <summary>Gets or creates a <see cref="DirectoryInfoConfig"/>.</summary>
        public static DirectoryInfoConfig GetDirectoryInfoConfig(string key, bool isTransient = false) => GetCustomConfig<DirectoryInfoConfig>(key, isTransient);
        /// <summary>Gets or creates a <see cref="FileInfoConfig"/>.</summary>
        public static FileInfoConfig GetFileInfoConfig(string key, bool isTransient = false) => GetCustomConfig<FileInfoConfig>(key, isTransient);
        /// <summary>Gets or creates a <see cref="UriConfig"/>.</summary>
        public static UriConfig GetUriConfig(string key, bool isTransient = false) => GetCustomConfig<UriConfig>(key, isTransient);
        /// <summary>Gets or creates an <see cref="IPAddressConfig"/>.</summary>
        public static IPAddressConfig GetIPAddressConfig(string key, bool isTransient = false) => GetCustomConfig<IPAddressConfig>(key, isTransient);
        /// <summary>Gets or creates a <see cref="VersionConfig"/>.</summary>
        public static VersionConfig GetVersionConfig(string key, bool isTransient = false) => GetCustomConfig<VersionConfig>(key, isTransient);
        /// <summary>Gets or creates an <see cref="EncodingConfig"/>.</summary>
        public static EncodingConfig GetEncodingConfig(string key, bool isTransient = false) => GetCustomConfig<EncodingConfig>(key, isTransient);
    }
}
