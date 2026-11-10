using System.Globalization;
using System.Windows.Input;

namespace Coding4Coffee.Basics.Localization.Commands
{
    /// <summary>
    /// Command to reset the application's culture to the operating system's default UI culture
    /// and clear any user-configured culture from the settings.
    /// </summary>
    /// <remarks>
    /// Monitors <see cref="LocalizationManager.CultureChanged"/> to raise <see cref="CanExecuteChanged"/> automatically.
    /// <see cref="CanExecute(object?)"/> evaluates to <see langword="true"/> when a custom culture is currently persisted in the configuration
    /// or when the current UI culture differs from the target system default culture.
    /// </remarks>
    public class ResetCultureToSystemDefaultCommand : ICommand
    {
        private readonly CultureInfo? _defaultFallbackCulture;

        /// <summary>
        /// Gets the singleton instance of the command using <see cref="CultureInfo.InstalledUICulture"/> as default fallback.
        /// </summary>
        public static ResetCultureToSystemDefaultCommand Instance { get; } = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="ResetCultureToSystemDefaultCommand"/> class.
        /// </summary>
        /// <param name="defaultFallbackCulture">
        /// Optional culture to use as fallback. If <see langword="null"/>, <see cref="CultureInfo.InstalledUICulture"/> is used.
        /// </param>
        public ResetCultureToSystemDefaultCommand(CultureInfo? defaultFallbackCulture = null)
        {
            _defaultFallbackCulture = defaultFallbackCulture;
            LocalizationManager.CultureChanged += (sender, e) => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Occurs when changes occur that affect whether or not the command should execute.
        /// </summary>
        public event EventHandler? CanExecuteChanged;

        /// <summary>
        /// Determines whether the command can execute in its current state.
        /// </summary>
        /// <param name="parameter">
        /// Optional fallback culture (as a <see cref="CultureInfo"/> or culture name <see cref="string"/>)
        /// that overrides the configured default fallback culture.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if a culture is saved in the configuration or the current UI culture differs from the target system culture;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        public bool CanExecute(object? parameter)
        {
            if (LocalizationManager.HasConfiguredCulture)
                return true;

            var targetCulture = ResolveFallbackCulture(parameter);
            return LocalizationManager.CurrentCulture.Name != targetCulture.Name;
        }

        /// <summary>
        /// Executes the command to reset the culture and clear settings.
        /// </summary>
        /// <param name="parameter">
        /// Optional fallback culture (as a <see cref="CultureInfo"/> or culture name <see cref="string"/>)
        /// that overrides the configured default fallback culture.
        /// </param>
        public void Execute(object? parameter)
        {
            if (!CanExecute(parameter))
                return;

            var targetCulture = ResolveFallbackCulture(parameter);
            LocalizationManager.ResetToSystemCulture(targetCulture);
        }

        /// <summary>
        /// Raises the <see cref="CanExecuteChanged"/> event.
        /// </summary>
        public void RaiseCanExecuteChanged()
            => CanExecuteChanged?.Invoke(this, EventArgs.Empty);

        private CultureInfo ResolveFallbackCulture(object? parameter)
        {
            if (parameter is CultureInfo cultureInfo)
                return cultureInfo;

            if (parameter is string cultureName && !string.IsNullOrWhiteSpace(cultureName))
            {
                try
                {
                    return CultureInfo.GetCultureInfo(cultureName);
                }
                catch
                {
                    // Fall back to default fallback if invalid culture name was passed
                }
            }

            return _defaultFallbackCulture ?? CultureInfo.InstalledUICulture;
        }
    }
}
