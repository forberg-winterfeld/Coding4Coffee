using System.Globalization;
using System.Windows.Input;

namespace Coding4Coffee.Basics.Localization.Commands
{
    /// <summary>
    /// Command to set the culture of the application.
    /// </summary>
    public class SetCultureCommand : ICommand
    {
        /// <summary>
        /// Gets the singleton instance of the command.
        /// </summary>
        public static SetCultureCommand Instance { get; } = new();

        /// <summary>
        /// Prevents a default instance of the <see cref="SetCultureCommand"/> class from being created.
        /// Subscribes to culture change events to update execution status.
        /// </summary>
        private SetCultureCommand()
        {
            LocalizationManager.CultureChanged += (sender, e) => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Occurs when changes occur that affect whether or not the command should execute.
        /// </summary>
        public event EventHandler? CanExecuteChanged;

        /// <summary>
        /// Determines whether the command can execute in its current state.
        /// </summary>
        /// <param name="parameter">The name of the culture to check (must be a non-empty string that differs from the current culture).</param>
        /// <returns><c>true</c> if the culture can be changed; otherwise, <c>false</c>.</returns>
        public bool CanExecute(object? parameter)
            => parameter is string cultureName && cultureName != LocalizationManager.CurrentCulture.Name;

        /// <summary>
        /// Executes the command to change the current application culture.
        /// </summary>
        /// <param name="parameter">The name of the culture to switch to.</param>
        public void Execute(object? parameter)
        {
            if (parameter is string cultureName)
                LocalizationManager.CurrentCulture = CultureInfo.GetCultureInfo(cultureName);
        }
    }
}
