using System.Globalization;
using Coding4Coffee.Basics.Configuration;

namespace Coding4Coffee.Basics.Localization
{
    /// <summary>
    /// Manages the application culture and notifies subscribers when it changes.
    /// </summary>
    public static class LocalizationManager
    {
        private static string _cultureConfigKey = string.Empty;
        private static CultureInfoConfig? _cultureConfig;
        private static bool _updatingConfig;

        /// <summary>
        /// Gets or sets the configuration key used to save/load the culture.
        /// </summary>
        /// <remarks>
        /// The configured culture is applied immediately and subsequent configuration changes are observed.
        /// An empty key removes the binding. Clearing the configuration value retains the current culture.
        /// Set the key and change its configuration on the UI thread when UI controls subscribe to culture changes.
        /// </remarks>
        /// <exception cref="InvalidOperationException">The key is already associated with a different configuration type.</exception>
        /// <exception cref="CultureNotFoundException">The configured culture name is invalid.</exception>
        public static string CultureConfigKey
        {
            get => _cultureConfigKey;
            set
            {
                if (_cultureConfigKey == value)
                    return;

                var config = string.IsNullOrWhiteSpace(value) ? null : ConfigFactory.GetCultureInfoConfig(value);
                var culture = config?.Value;
                _cultureConfig?.ValueChanged -= CultureConfig_ValueChanged;
                _cultureConfig = config;
                _cultureConfigKey = value;
                _cultureConfig?.ValueChanged += CultureConfig_ValueChanged;

                if (culture != null && ApplyCulture(culture))
                    CultureChanged?.Invoke(null, EventArgs.Empty);
            }
        }

        private static void CultureConfig_ValueChanged(object? sender, EventArgs e)
        {
            if (_updatingConfig)
                return;

            var culture = _cultureConfig?.Value;
            if (culture != null)
                ApplyCulture(culture);
            CultureChanged?.Invoke(null, EventArgs.Empty);
        }

        private static bool ApplyCulture(CultureInfo culture)
        {
            if (CultureInfo.CurrentUICulture.Name == culture.Name)
                return false;

            CultureInfo.CurrentCulture = culture;
            CultureInfo.CurrentUICulture = culture;
            return true;
        }

        private static void SetConfigValue(CultureInfo? culture)
        {
            if (_cultureConfig == null)
                return;

            bool wasUpdating = _updatingConfig;
            _updatingConfig = true;
            try
            {
                _cultureConfig.Value = culture;
            }
            finally
            {
                _updatingConfig = wasUpdating;
            }
        }

        /// <summary>
        /// Gets a value indicating whether a specific culture is saved in the configuration.
        /// Returns <see langword="false"/> if no culture has been explicitly configured.
        /// </summary>
        public static bool HasConfiguredCulture => _cultureConfig?.HasValue == true;

        /// <summary>
        /// Resets the culture to the system default UI culture and clears any configured culture from settings.
        /// </summary>
        /// <param name="fallbackCulture">
        /// Optional culture to apply as fallback. If <see langword="null"/>, <see cref="CultureInfo.InstalledUICulture"/> is used.
        /// </param>
        public static void ResetToSystemCulture(CultureInfo? fallbackCulture = null)
        {
            var targetCulture = fallbackCulture ?? CultureInfo.InstalledUICulture;

            bool cultureConfigCleared = _cultureConfig?.HasValue == true;
            if (cultureConfigCleared)
                SetConfigValue(null);

            bool cultureChanged = ApplyCulture(targetCulture);
            if (cultureChanged || cultureConfigCleared)
                CultureChanged?.Invoke(null, EventArgs.Empty);
        }

        /// <summary>
        /// Event raised when the current culture or its configured value changes.
        /// </summary>
        /// <remarks>Handlers run synchronously on the thread that made the change.</remarks>
        public static event EventHandler? CultureChanged;

        /// <summary>
        /// Gets or sets the current UI culture on the calling thread.
        /// </summary>
        public static CultureInfo CurrentCulture
        {
            get => CultureInfo.CurrentUICulture;
            set
            {
                if (ApplyCulture(value))
                {
                    SetConfigValue(value);
                    CultureChanged?.Invoke(null, EventArgs.Empty);
                }
            }
        }
    }
}

