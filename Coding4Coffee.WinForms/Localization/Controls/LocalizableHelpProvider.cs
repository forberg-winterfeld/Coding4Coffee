using Coding4Coffee.Basics.Localization;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Coding4Coffee.WinForms.Localization.Controls
{
    /// <summary>
    /// A HelpProvider component that supports dynamic localization of help strings
    /// assigned to controls at runtime using resource keys.
    /// </summary>
    /// <remarks>
    /// Uses <see cref="ConditionalWeakTable{TKey, TValue}"/> to store resource keys
    /// for individual controls, allowing garbage collection of controls without
    /// holding strong references. Override <see cref="GetHelpString(Control)"/> to
    /// retrieve localized help text via the associated resource manager.
    /// </remarks>
    [ProvideProperty("HelpResourceKey", typeof(Control))]
    public class LocalizableHelpProvider : HelpProvider
    {
        private readonly ConditionalWeakTable<Control, string> _resourceKeys = [];

        /// <summary>
        /// Retrieves the help string for a control, localizing it using the associated resource key.
        /// </summary>
        /// <remarks>
        /// If a resource key is registered for this control, the localized string is retrieved
        /// from the resource manager. If the key is not found or no resource manager is configured,
        /// the base help string is returned as a fallback.
        /// </remarks>
        /// <param name="control">The control for which to retrieve the help string.</param>
        /// <returns>The localized help string for the control, or the base help string if no localization is available.</returns>
        public override string? GetHelpString(Control control)
        {
            if (control != null && _resourceKeys.TryGetValue(control, out string? resourceKey))
                return this.GetResourceManager()?.GetString(resourceKey) ?? base.GetHelpString(control);

            return control != null ? base.GetHelpString(control) : null;
        }

        /// <summary>
        /// Retrieves the resource key associated with a control for localizing its help string.
        /// </summary>
        /// <param name="control">The control for which to retrieve the resource key.</param>
        /// <returns>The resource key if one is registered for this control; otherwise, <see langword="null"/>.</returns>
        [DisplayName("HelpResourceKey")]
        [Category("Localization")]
        [Description("The resource key used to retrieve the localized help string.")]
        public string? GetHelpResourceKey(Control control)
        {
            if (control != null && _resourceKeys.TryGetValue(control, out string? resourceKey))
            {
                return resourceKey;
            }
            return null;
        }

        /// <summary>
        /// Sets or clears the resource key associated with a control for localizing its help string.
        /// </summary>
        /// <remarks>
        /// If <paramref name="resourceKey"/> is <see langword="null"/> or empty, the registration for this control is removed.
        /// Otherwise, the key is registered for future lookups when <see cref="GetHelpString(Control)"/> is called.
        /// </remarks>
        /// <param name="control">The control to associate with the resource key.</param>
        /// <param name="resourceKey">The resource key to use for localization, or <see langword="null"/> to unregister.</param>
        public void SetHelpResourceKey(Control control, string? resourceKey)
        {
            if (control != null)
            {
                if (string.IsNullOrEmpty(resourceKey))
                {
                    UnbindHelpResourceKey(control);
                }
                else
                {
                    BindHelpResourceKey(control, resourceKey);
                }
            }
        }

        /// <summary>
        /// Registers a resource key for a control's help string and supports fluent method chaining.
        /// </summary>
        /// <remarks>
        /// If the resource key is empty or whitespace, the operation is silently ignored.
        /// The method uses <see cref="ConditionalWeakTable{TKey, TValue}.AddOrUpdate(TKey, TValue)"/>
        /// to avoid duplicate key errors when re-binding the same control.
        /// </remarks>
        /// <param name="control">The control to bind the resource key to.</param>
        /// <param name="resourceKey">The resource key to use for localizing the help string.</param>
        /// <returns>The current <see cref="LocalizableHelpProvider"/> instance to support fluent method chaining.</returns>
        public LocalizableHelpProvider BindHelpResourceKey(Control control, string resourceKey)
        {
            if (control != null && !string.IsNullOrEmpty(resourceKey))
            {
                _resourceKeys.AddOrUpdate(control, resourceKey);
            }

            return this;
        }

        /// <summary>
        /// Unregisters a resource key for a control's help string and supports fluent method chaining.
        /// </summary>
        /// <remarks>
        /// If the control has no registered resource key, the operation is silently ignored.
        /// Once unbound, <see cref="GetHelpString(Control)"/> will return the base help string without localization.
        /// </remarks>
        /// <param name="control">The control to unbind the resource key from.</param>
        /// <returns>The current <see cref="LocalizableHelpProvider"/> instance to support fluent method chaining.</returns>
        public LocalizableHelpProvider UnbindHelpResourceKey(Control control)
        {
            if (control != null)
                _resourceKeys.Remove(control);

            return this;
        }
    }
}